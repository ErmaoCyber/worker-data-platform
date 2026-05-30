using Microsoft.EntityFrameworkCore;
using wdb_backend.Abstractions;
using wdb_backend.Common;
using wdb_backend.Data;
using wdb_backend.DTOs;

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
            .FirstOrDefaultAsync(e => e.Id == employerId, cancellationToken)
            ?? throw new UnauthorizedAccessException("Current user is not an employer.");

        var now = DateTime.UtcNow;
        var recentRequests = await GetRecentRequestsAsync(employerId, now, cancellationToken);

        return new EmployerDashboardDto
        {
            Company = new EmployerCompanyInfoDto
            {
                Name = employer.Name,
                Email = employer.Email,
                Verified = employer.Verified
            },
            Summary = new EmployerDashboardSummaryDto
            {
                PendingRequests = recentRequests.Count(r => r.Status == "Pending"),
                AvailableRequests = recentRequests.Count(r => r.Status == "Available"),
                PartialRequests = recentRequests.Count(r => r.Status == "Partial")
            },
            RecentRequests = recentRequests
        };
    }

    private async Task<List<EmployerRecentRequestDto>> GetRecentRequestsAsync(
        Guid employerId,
        DateTime now,
        CancellationToken cancellationToken)
    {
        // One row per permission item; we group by request below
        var rows = await (
            from request in _context.Requests.AsNoTracking()
            join worker in _context.Workers.AsNoTracking() on request.WorkerId equals worker.Id
            join permission in _context.Permissions.AsNoTracking() on request.Id equals permission.RequestId
            join workerInfo in _context.WorkerInfos.AsNoTracking() on permission.InfoId equals workerInfo.Id into wiGroup
            from workerInfo in wiGroup.DefaultIfEmpty()
            where request.EmployerId == employerId
            orderby request.CreatedAt descending
            select new
            {
                request.Id,
                WorkerName = worker.Name,
                request.Reason,
                PermStatus = permission.Status,
                ExpiryDate = request.ExpiryDate,
                LastUpdated = permission.LastUpdatedAt ?? DateTime.MinValue,
                FieldLabel = workerInfo == null
                                    ? "Pending"
                                    : (workerInfo.CustomLabel ?? workerInfo.Field!.Label)
            }
        ).Take(50).ToListAsync(cancellationToken);

        return rows
            .GroupBy(r => r.Id)
            .Select(group =>
            {
                var items = group.ToList();
                var allStatuses = items.Select(i => i.PermStatus).ToList();

                string status;
                if (allStatuses.All(s => s == PermissionStatus.Pending))
                    status = "Pending";
                else if (allStatuses.Any(s => s == PermissionStatus.Revoked))
                    status = "Revoked";
                else if (allStatuses.Any(s => s == PermissionStatus.Approved) &&
                         allStatuses.Any(s => s is PermissionStatus.Pending or PermissionStatus.Rejected))
                    status = "Partial";
                else if (allStatuses.All(s => s == PermissionStatus.Approved))
                    status = "Available";
                else
                    status = "Rejected";

                return new EmployerRecentRequestDto
                {
                    RequestId = group.Key,
                    WorkerName = items.First().WorkerName,
                    RequestedFields = items.Select(i => i.FieldLabel).Distinct().ToList(),
                    Reason = items.First().Reason,
                    Status = status,
                    LastUpdatedAt = items.Max(i => i.LastUpdated)
                };
            })
            .Take(5)
            .ToList();
    }
}
