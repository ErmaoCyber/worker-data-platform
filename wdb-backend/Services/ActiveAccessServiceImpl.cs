using Microsoft.EntityFrameworkCore;
using wdb_backend.Abstractions;
using wdb_backend.Common;
using wdb_backend.Data;
using wdb_backend.DTOs;

namespace wdb_backend.Services;

public class ActiveAccessServiceImpl : IActiveAccessService
{
    private readonly AppDbContext _context;

    public ActiveAccessServiceImpl(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Return active approved permissions for the worker.
    /// A permission is active only when:
    /// - permission.status = Approved
    /// - related request has not expired
    /// </summary>
    public async Task<List<ActiveAccessDto>> GetActiveAccessAsync(
        Guid workerId,
        string? company = null,
        string? dataType = null)
    {
        var now = DateTime.UtcNow;

        var requests = await _context.Requests
            .Where(r =>
                r.WorkerId == workerId &&
                r.ExpiryDate > now &&
                r.Permissions.Any(p => p.Status == PermissionStatus.Approved))
            .Include(r => r.Permissions)
                .ThenInclude(p => p.WorkerInfo)
                    .ThenInclude(w => w!.Field)
                        .ThenInclude(f => f!.Category)
            .Include(r => r.Permissions)
                .ThenInclude(p => p.Field)
                    .ThenInclude(f => f!.Category)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        var rows = new List<ActiveAccessDto>();

        foreach (var request in requests)
        {
            var employer = await _context.Employers
                .FirstOrDefaultAsync(e => e.Id == request.EmployerId);

            var companyName = employer?.Name ?? "Unknown";

            if (!string.IsNullOrWhiteSpace(company) &&
                !companyName.Contains(company, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var approvedPermissions = request.Permissions
                .Where(p => p.Status == PermissionStatus.Approved)
                .ToList();

            var infoItems = approvedPermissions
                .Select(p =>
                {
                    var label =
                        p.Field?.Label ??
                        p.WorkerInfo?.Field?.Label ??
                        p.WorkerInfo?.CustomLabel ??
                        "Unknown";

                    return new ActiveAccessInfoDto
                    {
                        PermissionId = p.Id,
                        DataType = label
                    };
                })
                .ToList();

            if (!string.IsNullOrWhiteSpace(dataType))
            {
                infoItems = infoItems
                    .Where(i => i.DataType.Equals(dataType, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                if (!infoItems.Any())
                    continue;
            }

            rows.Add(new ActiveAccessDto
            {
                RequestId = request.Id,
                CompanyName = companyName,
                GrantedAt = approvedPermissions.Max(p => p.LastUpdatedAt) ?? DateTime.UtcNow,
                Reason = request.Reason,
                WorkerInfo = infoItems
            });
        }

        return rows
            .OrderByDescending(r => r.GrantedAt)
            .ToList();
    }

    /// <summary>
    /// Revoke one approved active permission.
    /// This updates database status and creates an ACCESS_REVOKED notification for the employer.
    /// Blockchain side effects can be added later.
    /// </summary>
    public async Task RevokePermissionAsync(
        Guid workerId,
        Guid permissionId,
        CancellationToken cancellationToken = default)
    {
        var permission = await _context.Permissions
            .Include(p => p.Request)
            .FirstOrDefaultAsync(
                p => p.Id == permissionId && p.WorkerId == workerId,
                cancellationToken)
            ?? throw new KeyNotFoundException("PERMISSION_NOT_FOUND");

        if (permission.Request.WorkerId != workerId)
            throw new UnauthorizedAccessException("PERMISSION_NOT_OWNED_BY_WORKER");

        if (permission.Status != PermissionStatus.Approved)
            throw new InvalidOperationException("PERMISSION_NOT_APPROVED");

        if (permission.Request.ExpiryDate <= DateTime.UtcNow)
            throw new InvalidOperationException("REQUEST_EXPIRED");

        permission.Status = PermissionStatus.Revoked;
        permission.LastUpdatedAt = DateTime.UtcNow;

        // Notify the employer that the worker revoked access.
        _context.Notifications.Add(new wdb_backend.Models.Notification
        {
            RecipientWorkerId = null,
            RecipientEmployerId = permission.Request.EmployerId,
            Type = "ACCESS_REVOKED",
            RequestId = permission.RequestId,
            IsRead = false
        });

        await _context.SaveChangesAsync(cancellationToken);
    }
}
