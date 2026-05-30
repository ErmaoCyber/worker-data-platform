using Microsoft.EntityFrameworkCore;
using wdb_backend.Abstractions;
using wdb_backend.Common;
using wdb_backend.Data;
using wdb_backend.DTOs;

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
            throw new UnauthorizedAccessException("Current user is not an employer.");

        var now = DateTime.UtcNow;

        // Left join to workerInfo — info_id may be null for pending preset permissions
        var requestRows = await (
            from request in _context.Requests.AsNoTracking()
            join worker in _context.Workers.AsNoTracking() on request.WorkerId equals worker.Id
            join permission in _context.Permissions.AsNoTracking() on request.Id equals permission.RequestId
            join workerInfo in _context.WorkerInfos.AsNoTracking() on permission.InfoId equals workerInfo.Id into wiGroup
            from workerInfo in wiGroup.DefaultIfEmpty()
            where request.EmployerId == employerId
            select new SentRequestRow
            {
                RequestId = request.Id,
                WorkerId = worker.Id,
                WorkerName = worker.Name,
                WorkerEmail = worker.Email,
                Reason = request.Reason,
                RequestedAt = request.CreatedAt,
                // Use CustomLabel for custom fields, Field.Label for preset; fall back to "Pending"
                DataType = workerInfo == null
                                    ? "Pending"
                                    : (workerInfo.CustomLabel ?? workerInfo.Field!.Label),
                PermissionStatus = permission.Status,
                ExpiryDate = request.ExpiryDate,   // moved from permission to request
                LastUpdatedAt = permission.LastUpdatedAt ?? DateTime.MinValue
            }
        ).ToListAsync(cancellationToken);

        return requestRows
            .GroupBy(row => row.RequestId)
            .Select(group =>
            {
                var rows = group.ToList();
                return new EmployerSentRequestDto
                {
                    RequestId = group.Key,
                    WorkerId = rows.First().WorkerId,
                    WorkerName = rows.First().WorkerName,
                    WorkerEmail = rows.First().WorkerEmail,
                    Reason = rows.First().Reason,
                    RequestedAt = rows.First().RequestedAt,
                    LastUpdatedAt = rows.Max(r => r.LastUpdatedAt),
                    Status = GetRequestStatus(rows, now),
                    RequestedDataTypes = rows.Select(r => r.DataType).Distinct().ToList()
                };
            })
            .OrderByDescending(r => r.LastUpdatedAt)
            .ToList();
    }

    private static string GetRequestStatus(List<SentRequestRow> rows, DateTime now)
    {
        if (rows.Count == 0) return "Unknown";

        if (rows.All(r => r.PermissionStatus == PermissionStatus.Pending)) return "Pending";
        if (rows.All(r => r.PermissionStatus == PermissionStatus.Rejected)) return "Rejected";
        if (rows.Any(r => r.PermissionStatus == PermissionStatus.Revoked)) return "Revoked";

        if (rows.All(r => r.PermissionStatus == PermissionStatus.Approved && r.ExpiryDate > now))
            return "Approved";

        if (rows.All(r => r.PermissionStatus == PermissionStatus.Approved && r.ExpiryDate <= now))
            return "Expired";

        return "Partial";
    }

    private class SentRequestRow
    {
        public Guid RequestId { get; set; }
        public Guid WorkerId { get; set; }
        public string WorkerName { get; set; } = string.Empty;
        public string WorkerEmail { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public DateTime RequestedAt { get; set; }
        public string DataType { get; set; } = string.Empty;
        public int PermissionStatus { get; set; }
        public DateTime ExpiryDate { get; set; }
        public DateTime LastUpdatedAt { get; set; }
    }
}
