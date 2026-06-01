using Microsoft.EntityFrameworkCore;
using wdb_backend.Abstractions;
using wdb_backend.Common;
using wdb_backend.Data;
using wdb_backend.DTOs;
using wdb_backend.Models;

namespace wdb_backend.Services;

public class EmployerRequestServiceImpl : IEmployerRequestService
{
    private readonly AppDbContext _context;
    private readonly INotificationService _notificationService;
    private readonly IBlockchainAuditService _audit;

    public EmployerRequestServiceImpl(
        AppDbContext context,
        INotificationService notificationService,
        IBlockchainAuditService audit)
    {
        _context = context;
        _notificationService = notificationService;
        _audit = audit;
    }

    public async Task<EmployerRequestCatalogDto?> GetCatalogAsync(
        string workerEmail,
        CancellationToken cancellationToken = default)
    {
        var worker = await _context.Workers
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.Email == workerEmail, cancellationToken);

        if (worker == null)
        {
            return null;
        }

        var categories = await _context.Categories
            .AsNoTracking()
            .Include(c => c.Fields)
            .ToListAsync(cancellationToken);

        // Worker's custom items live under the OtherInformation category by convention;
        // they are identified by having a non-null custom_label.
        var customItems = await _context.WorkerInfos
            .AsNoTracking()
            .Where(wi => wi.WorkerId == worker.Id && wi.CustomLabel != null)
            .ToListAsync(cancellationToken);

        var otherCategory = categories.FirstOrDefault(c => c.CategoryName == "OtherInformation");

        return new EmployerRequestCatalogDto
        {
            Worker = new EmployerRequestCatalogWorkerDto
            {
                Id = worker.Id,
                Name = worker.Name,
                Email = worker.Email
            },
            Categories = categories.Select(c => new EmployerRequestCatalogCategoryDto
            {
                Id = c.Id,
                Name = c.CategoryName,
                PresetFields = c.Fields
                    .OrderBy(f => f.Label)
                    .Select(f => new EmployerRequestCatalogPresetFieldDto
                    {
                        FieldId = f.Id,
                        Label = f.Label,
                        AllowedType = f.AllowedType
                    })
                    .ToList(),
                CustomItems = (otherCategory != null && c.Id == otherCategory.Id)
                    ? customItems.Select(wi => new EmployerRequestCatalogCustomItemDto
                    {
                        WorkerInfoId = wi.Id,
                        Label = wi.CustomLabel ?? string.Empty,
                        Type = wi.Type
                    }).ToList()
                    : new List<EmployerRequestCatalogCustomItemDto>()
            }).ToList()
        };
    }

    public async Task<CreateEmployerRequestResultDto> CreateAsync(
        Guid employerId,
        CreateEmployerRequestDto dto,
        CancellationToken cancellationToken = default)
    {
        var worker = await _context.Workers
            .FirstOrDefaultAsync(w => w.Email == dto.WorkerEmail, cancellationToken)
            ?? throw new KeyNotFoundException($"Worker {dto.WorkerEmail} not found");

        if (dto.ExpiryDate <= DateTime.UtcNow)
        {
            throw new ArgumentException("Expiry date must be in the future");
        }

        var hasAnyItem = dto.PresetFieldIds.Count > 0
                         || dto.CustomWorkerInfoIds.Count > 0
                         || !string.IsNullOrWhiteSpace(dto.CustomRequest);
        if (!hasAnyItem)
        {
            throw new ArgumentException(
                "Request must include at least one field, custom item, or custom request");
        }

        var hasCustomRequest = !string.IsNullOrWhiteSpace(dto.CustomRequest);
        var request = new Request
        {
            EmployerId = employerId,
            WorkerId = worker.Id,
            Reason = dto.Reason,
            ExpiryDate = dto.ExpiryDate,
            CustomRequest = hasCustomRequest ? dto.CustomRequest : null,
            CustomRequestStatus = hasCustomRequest ? "pending" : null
        };
        _context.Requests.Add(request);
        await _context.SaveChangesAsync(cancellationToken);

        // Preset fields: permission references the field definition; info_id stays null
        // until the worker approves and an actual worker_info row is associated.
        foreach (var fieldId in dto.PresetFieldIds.Distinct())
        {
            _context.Permissions.Add(new Permission
            {
                RequestId = request.Id,
                WorkerId = worker.Id,
                FieldId = fieldId,
                InfoId = null,
                Status = PermissionStatus.Pending,
                LastUpdatedAt = DateTime.UtcNow
            });
        }

        // Existing custom items: permission points directly at the worker_info row.
        foreach (var workerInfoId in dto.CustomWorkerInfoIds.Distinct())
        {
            _context.Permissions.Add(new Permission
            {
                RequestId = request.Id,
                WorkerId = worker.Id,
                FieldId = null,
                InfoId = workerInfoId,
                Status = PermissionStatus.Pending,
                LastUpdatedAt = DateTime.UtcNow
            });
        }

        await _context.SaveChangesAsync(cancellationToken);

        await _notificationService.NotifyAsync(
            NotificationType.NewRequest,
            recipientWorkerId: worker.Id,
            recipientEmployerId: null,
            requestId: request.Id,
            cancellationToken);

        await _audit.TryLogAsync(
            employerId,
            worker.Id,
            BlockchainAction.RequestCreated,
            cancellationToken);

        return new CreateEmployerRequestResultDto { RequestId = request.Id };
    }
}
