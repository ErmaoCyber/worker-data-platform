using Microsoft.EntityFrameworkCore;
using wdb_backend.Abstractions;
using wdb_backend.Common;
using wdb_backend.Data;
using wdb_backend.Models;

namespace wdb_backend.Usecases;

/// <summary>
/// Creates a data access request and the related permission rows.
/// This version supports the new permission model:
/// - preset field request: field_id set, info_id null
/// - custom field request: info_id set, field_id null
/// </summary>
public class CreateDataAccessRequestUsecaseImpl : ICreateDataAccessRequestUsecase
{
    private readonly AppDbContext _context;
    private readonly IRequestService _requestService;

    public CreateDataAccessRequestUsecaseImpl(
        AppDbContext context,
        IRequestService requestService)
    {
        _context = context;
        _requestService = requestService;
    }

    public async Task CreateDataAccessRequest(
        List<Guid> selectedItemIds,
        Guid employerId,
        Guid workerId,
        string reason,
        CancellationToken cancellationToken = default)
    {
        if (selectedItemIds == null || selectedItemIds.Count == 0)
            throw new InvalidOperationException("NO_SELECTED_ITEMS");

        if (string.IsNullOrWhiteSpace(reason))
            throw new InvalidOperationException("REASON_REQUIRED");

        var distinctIds = selectedItemIds.Distinct().ToList();

        var request = await _requestService.CreateAsync(
            employerId,
            workerId,
            reason.Trim(),
            cancellationToken);

        foreach (var selectedId in distinctIds)
        {
            // 1. Custom or existing worker_info row.
            var workerInfo = await _context.WorkerInfos
                .Include(w => w.Field)
                .FirstOrDefaultAsync(
                    w => w.Id == selectedId && w.WorkerId == workerId,
                    cancellationToken);

            if (workerInfo != null)
            {
                await EnsureNotAlreadyRequestedAsync(
                    employerId,
                    workerId,
                    workerInfo.FieldId,
                    workerInfo.Id,
                    cancellationToken);

                var permissionForExistingInfo = new Permission
                {
                    RequestId = request.Id,
                    WorkerId = workerId,
                    FieldId = workerInfo.FieldId,
                    InfoId = workerInfo.Id,
                    Status = PermissionStatus.Pending,
                    LastUpdatedAt = DateTime.UtcNow
                };

                _context.Permissions.Add(permissionForExistingInfo);
                continue;
            }

            // 2. Preset field definition.
            var field = await _context.Fields
                .FirstOrDefaultAsync(f => f.Id == selectedId, cancellationToken);

            if (field != null)
            {
                await EnsureNotAlreadyRequestedAsync(
                    employerId,
                    workerId,
                    field.Id,
                    null,
                    cancellationToken);

                var permissionForPresetField = new Permission
                {
                    RequestId = request.Id,
                    WorkerId = workerId,
                    FieldId = field.Id,
                    InfoId = null,
                    Status = PermissionStatus.Pending,
                    LastUpdatedAt = DateTime.UtcNow
                };

                _context.Permissions.Add(permissionForPresetField);
                continue;
            }

            throw new KeyNotFoundException("SELECTED_ITEM_NOT_FOUND");
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task EnsureNotAlreadyRequestedAsync(
        Guid employerId,
        Guid workerId,
        Guid? fieldId,
        Guid? infoId,
        CancellationToken cancellationToken)
    {
        var alreadyRequested = await _context.Permissions
            .Include(p => p.Request)
            .AnyAsync(
                p =>
                    p.WorkerId == workerId &&
                    p.Request.EmployerId == employerId &&
                    (p.Status == PermissionStatus.Pending ||
                     p.Status == PermissionStatus.Approved) &&
                    (
                        (fieldId.HasValue && p.FieldId == fieldId.Value) ||
                        (infoId.HasValue && p.InfoId == infoId.Value)
                    ),
                cancellationToken);

        if (alreadyRequested)
            throw new InvalidOperationException("ITEM_ALREADY_REQUESTED");
    }
}
