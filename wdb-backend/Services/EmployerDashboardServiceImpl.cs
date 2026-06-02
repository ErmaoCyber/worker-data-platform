using Microsoft.EntityFrameworkCore;
using wdb_backend.Abstractions;
using wdb_backend.Common;
using wdb_backend.Data;
using wdb_backend.DTOs;
using wdb_backend.Models;

namespace wdb_backend.Services;

public class EmployerDashboardServiceImpl : IEmployerDashboardService
{
    private readonly AppDbContext _context;

    public EmployerDashboardServiceImpl(AppDbContext context)
    {
        _context = context;
    }

    public async Task<EmployerDashboardDto> GetDashboardAsync(
        Guid employerId,
        CancellationToken cancellationToken = default)
    {
        var employer = await _context.Employers
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == employerId, cancellationToken);

        if (employer == null)
        {
            throw new UnauthorizedAccessException("Current user is not an employer.");
        }

        var now = DateTime.UtcNow;

        // Pull employer's requests once with everything the summary counts and the
        // recent-requests list need (worker, permissions, field/category, custom items).
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

        // Compute display status once per request and reuse it for the summary counts.
        var perRequestStatus = requests.ToDictionary(r => r.Id, GetRequestStatus);

        var summary = new EmployerDashboardSummaryDto
        {
            PendingRequests = perRequestStatus.Values.Count(s => s == "Pending"),
            PartiallyApprovedRequests = perRequestStatus.Values.Count(s => s == "PartiallyApproved"),
            ApprovedRequests = perRequestStatus.Values.Count(s => s == "Approved"),
            ActiveAccessCount = requests.Count(r =>
                r.ExpiryDate > now
                && r.Permissions.Any(p => p.Status == PermissionStatus.Approved))
        };

        var recentRequests = requests
            .Take(8)
            .Select(r => new EmployerRecentRequestDto
            {
                RequestId = r.Id,
                WorkerName = r.Worker.Name,
                RequestedFields = r.Permissions
                    .Select(GetPermissionLabel)
                    .Distinct()
                    .ToList(),
                Reason = r.Reason,
                Status = perRequestStatus[r.Id],
                LastUpdatedAt = r.Permissions.Any()
                    ? (r.Permissions.Max(p => p.LastUpdatedAt) ?? r.CreatedAt)
                    : r.CreatedAt
            })
            .ToList();

        return new EmployerDashboardDto
        {
            Company = new EmployerCompanyInfoDto
            {
                Name = employer.Name,
                Email = employer.Email,
                Verified = employer.Verified
            },
            Summary = summary,
            RecentRequests = recentRequests
        };
    }

    // Resolve a display label for a permission. Preset permissions come from
    // Field.Label; custom-item permissions come from the linked worker_info row.
    private static string GetPermissionLabel(Permission p)
    {
        if (p.Field != null) return p.Field.Label;
        if (p.WorkerInfo != null)
        {
            return p.WorkerInfo.CustomLabel
                   ?? p.WorkerInfo.Field?.Label
                   ?? "Unknown";
        }
        return "Unknown";
    }

    // Badge rules per the design doc:
    //   any revoked                                       -> Revoked
    //   all pending                                       -> Pending
    //   all rejected                                      -> Rejected
    //   all approved + custom_request_status != rejected  -> Approved
    //   otherwise (mix of states)                         -> PartiallyApproved
    private static string GetRequestStatus(Request request)
    {
        var perms = request.Permissions;
        if (perms.Count == 0)
        {
            // No permissions yet (only a free-text custom_request not yet acted on).
            // Treat as Pending so the request still shows up in that bucket.
            return "Pending";
        }

        if (perms.Any(p => p.Status == PermissionStatus.Revoked))
        {
            return "Revoked";
        }
        if (perms.All(p => p.Status == PermissionStatus.Pending))
        {
            return "Pending";
        }
        if (perms.All(p => p.Status == PermissionStatus.Rejected))
        {
            return "Rejected";
        }
        if (perms.All(p => p.Status == PermissionStatus.Approved)
            && request.CustomRequestStatus != "rejected")
        {
            return "Approved";
        }
        return "PartiallyApproved";
    }
}
