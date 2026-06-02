using Microsoft.EntityFrameworkCore;
using wdb_backend.Abstractions;
using wdb_backend.Data;
using wdb_backend.DTOs;
using wdb_backend.Models;

namespace wdb_backend.Services;

public class EmployerSentRequestServiceImpl : IEmployerSentRequestService
{
    private readonly AppDbContext _context;

    public EmployerSentRequestServiceImpl(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<EmployerSentRequestDto>> GetSentRequestsAsync(
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

        // Pull requests with permissions and both possible label sources
        // (preset field via Permission.Field, or worker-created item via
        // Permission.WorkerInfo). Pulling everything in a single query
        // keeps the per-item resolution simple in memory.
        var requests = await _context.Requests
            .AsNoTracking()
            .Include(r => r.Worker)
            .Include(r => r.Permissions)
                .ThenInclude(p => p.Field!)
                    .ThenInclude(f => f.Category)
            .Include(r => r.Permissions)
                .ThenInclude(p => p.WorkerInfo!)
                    .ThenInclude(wi => wi.Field!)
                        .ThenInclude(f => f.Category)
            .Where(r => r.EmployerId == employerId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);

        return requests.Select(r => new EmployerSentRequestDto
        {
            RequestId = r.Id,
            WorkerId = r.WorkerId,
            WorkerName = r.Worker.Name,
            WorkerEmail = r.Worker.Email,
            Reason = r.Reason,
            ExpiryDate = r.ExpiryDate,
            CreatedAt = r.CreatedAt,
            LastUpdatedAt = r.Permissions.Any()
                ? (r.Permissions.Max(p => p.LastUpdatedAt) ?? r.CreatedAt)
                : r.CreatedAt,
            CustomRequest = r.CustomRequest,
            CustomRequestStatus = r.CustomRequestStatus,
            Items = r.Permissions.Select(BuildItem).ToList()
        }).ToList();
    }

    // Resolve label + category from either side of the dual-pointer permission.
    private static EmployerSentRequestItemDto BuildItem(Permission p)
    {
        if (p.Field != null)
        {
            return new EmployerSentRequestItemDto
            {
                PermissionId = p.Id,
                CategoryName = p.Field.Category?.CategoryName ?? "Unknown",
                Label = p.Field.Label,
                Status = p.Status,
                IsCustom = false
            };
        }

        if (p.WorkerInfo != null)
        {
            var label = p.WorkerInfo.CustomLabel
                        ?? p.WorkerInfo.Field?.Label
                        ?? "Unknown";
            var category = p.WorkerInfo.Field?.Category?.CategoryName
                           ?? "OtherInformation";
            return new EmployerSentRequestItemDto
            {
                PermissionId = p.Id,
                CategoryName = category,
                Label = label,
                Status = p.Status,
                IsCustom = p.WorkerInfo.CustomLabel != null
            };
        }

        return new EmployerSentRequestItemDto
        {
            PermissionId = p.Id,
            CategoryName = "Unknown",
            Label = "Unknown",
            Status = p.Status,
            IsCustom = false
        };
    }
}
