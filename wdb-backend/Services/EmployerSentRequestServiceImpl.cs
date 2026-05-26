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
            .AnyAsync(employer => employer.Id == employerId, cancellationToken);

        if (!employerExists)
        {
            throw new UnauthorizedAccessException("Current user is not an employer.");
        }

        var requestRows = await (
            from request in _context.Requests.AsNoTracking()
            join worker in _context.Workers.AsNoTracking()
                on request.WorkerId equals worker.Id
            join permission in _context.Permissions.AsNoTracking()
                on request.Id equals permission.RequestId
            join workerInfo in _context.WorkerInfos.AsNoTracking()
                on permission.InfoId equals workerInfo.Id
            where request.EmployerId == employerId
            select new SentRequestRow
            {
                RequestId = request.Id,
                WorkerId = worker.Id,
                WorkerName = worker.Name,
                WorkerEmail = worker.Email,
                Reason = request.Reason,
                RequestedAt = request.CreatedAt,
                DataType = workerInfo.Desc,
                PermissionStatus = permission.Status,
                ExpiryDate = permission.ExpiryDate,
                LastUpdatedAt = permission.LastUpdatedAt
            }
        ).ToListAsync(cancellationToken);

        var now = DateTime.UtcNow;

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
                    LastUpdatedAt = rows.Max(row => row.LastUpdatedAt),
                    Status = GetRequestStatus(rows, now),
                    RequestedDataTypes = rows
                        .Select(row => row.DataType)
                        .Distinct()
                        .ToList()
                };
            })
            .OrderByDescending(request => request.LastUpdatedAt)
            .ToList();
    }

    private static string GetRequestStatus(
        List<SentRequestRow> rows,
        DateTime now)
    {
        if (rows.Count == 0)
        {
            return "Unknown";
        }

        if (rows.All(row => row.PermissionStatus == PermissionStatus.Pending))
        {
            return "Pending";
        }

        if (rows.All(row => row.PermissionStatus == PermissionStatus.Rejected))
        {
            return "Rejected";
        }

        if (rows.All(row =>
            row.PermissionStatus == PermissionStatus.Approved &&
            (!row.ExpiryDate.HasValue || row.ExpiryDate.Value > now)))
        {
            return "Approved";
        }

        if (rows.All(row =>
            row.PermissionStatus == PermissionStatus.Approved &&
            row.ExpiryDate.HasValue &&
            row.ExpiryDate.Value <= now))
        {
            return "Expired";
        }

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

        public PermissionStatus PermissionStatus { get; set; }

        public DateTime? ExpiryDate { get; set; }

        public DateTime LastUpdatedAt { get; set; }
    }
}
