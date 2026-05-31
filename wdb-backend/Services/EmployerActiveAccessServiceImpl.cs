using Microsoft.EntityFrameworkCore;
using wdb_backend.Abstractions;
using wdb_backend.Common;
using wdb_backend.Data;
using wdb_backend.DTOs;

namespace wdb_backend.Services;

public class EmployerActiveAccessServiceImpl : IEmployerActiveAccessService
{
    private readonly AppDbContext _context;
    private readonly ISupabaseStorageService _storage;

    public EmployerActiveAccessServiceImpl(AppDbContext context, ISupabaseStorageService storage)
    {
        _context = context;
        _storage = storage;
    }

    public async Task<List<EmployerActiveAccessDto>> GetActiveAccessAsync(
        Guid employerId,
        CancellationToken cancellationToken = default)
    {
        var employerExists = await _context.Employers
            .AsNoTracking()
            .AnyAsync(e => e.Id == employerId, cancellationToken);

        if (!employerExists)
        {
            throw new UnauthorizedAccessException("Current user is not an employer.");
        }

        var now = DateTime.UtcNow;

        var requests = await _context.Requests
            .AsNoTracking()
            .Include(r => r.Worker)
            .Include(r => r.Permissions)
                .ThenInclude(p => p.WorkerInfo!)
                    .ThenInclude(wi => wi.Field!)
                        .ThenInclude(f => f.Category)
            .Where(r => r.EmployerId == employerId && r.ExpiryDate > now)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);

        var result = new List<EmployerActiveAccessDto>();

        foreach (var request in requests)
        {
            // Only approved permissions that already have a worker_info row attached.
            // Pending / rejected / revoked rows do not appear in My Access.
            var approvedPerms = request.Permissions
                .Where(p => p.Status == PermissionStatus.Approved && p.WorkerInfo != null)
                .ToList();
            if (approvedPerms.Count == 0) continue;

            var groups = approvedPerms
                .Select(p => new
                {
                    Permission = p,
                    CategoryName = p.WorkerInfo!.Field?.Category?.CategoryName
                                   ?? "OtherInformation",
                    Label = p.WorkerInfo.CustomLabel
                            ?? p.WorkerInfo.Field?.Label
                            ?? "Unknown",
                    IsCustom = p.WorkerInfo.CustomLabel != null
                })
                .GroupBy(x => x.CategoryName)
                .Select(g => new EmployerActiveAccessCategoryDto
                {
                    Name = g.Key,
                    Items = g.Select(x => new EmployerActiveAccessItemDto
                    {
                        PermissionId = x.Permission.Id,
                        Label = x.Label,
                        Type = x.Permission.WorkerInfo!.Type,
                        IsCustom = x.IsCustom
                    }).ToList()
                })
                .ToList();

            result.Add(new EmployerActiveAccessDto
            {
                RequestId = request.Id,
                WorkerId = request.WorkerId,
                WorkerName = request.Worker.Name,
                WorkerEmail = request.Worker.Email,
                Reason = request.Reason,
                GrantedAt = approvedPerms.Max(p => p.LastUpdatedAt),
                ExpiryDate = request.ExpiryDate,
                Categories = groups
            });
        }

        return result;
    }

    public async Task<EmployerAccessViewResultDto> ViewAsync(
        Guid employerId,
        Guid permissionId,
        CancellationToken cancellationToken = default)
    {
        var permission = await _context.Permissions
            .AsNoTracking()
            .Include(p => p.Request)
            .Include(p => p.WorkerInfo)
            .FirstOrDefaultAsync(p => p.Id == permissionId, cancellationToken)
            ?? throw new KeyNotFoundException($"Permission {permissionId} not found");

        if (permission.Request.EmployerId != employerId)
        {
            throw new UnauthorizedAccessException("Permission does not belong to the current employer.");
        }

        if (permission.Status != PermissionStatus.Approved)
        {
            throw new InvalidOperationException("Permission is not approved.");
        }

        if (permission.Request.ExpiryDate <= DateTime.UtcNow)
        {
            throw new InvalidOperationException("Permission has expired.");
        }

        if (permission.WorkerInfo == null)
        {
            throw new InvalidOperationException("No data associated with this permission yet.");
        }

        var info = permission.WorkerInfo;

        // TODO (task #16): publish DATA_ACCESSED notification to the worker.
        // TODO (task #17): log DATA_ACCESSED on-chain.

        if (info.Type == "file")
        {
            if (string.IsNullOrWhiteSpace(info.Value))
            {
                throw new InvalidOperationException("No file path stored for this item.");
            }
            var signed = await _storage.CreateSignedUrlAsync(info.Value, 900, cancellationToken);
            return new EmployerAccessViewResultDto
            {
                Type = "file",
                Url = signed.Url,
                UrlExpiresAt = signed.ExpiresAt
            };
        }

        return new EmployerAccessViewResultDto
        {
            Type = "text",
            Value = info.Value
        };
    }
}
