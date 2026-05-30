using Microsoft.EntityFrameworkCore;
using wdb_backend.Abstractions;
using wdb_backend.Common;
using wdb_backend.Data;
using wdb_backend.DTOs;

namespace wdb_backend.Services;

public class EmployerActiveAccessServiceImpl : IEmployerActiveAccessService
{
    private readonly AppDbContext _context;

    public EmployerActiveAccessServiceImpl(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<EmployerActiveAccessDto>> GetActiveAccessAsync(
        Guid employerId,
        CancellationToken cancellationToken = default)
    {
        var employerExists = await _context.Employers
            .AsNoTracking()
            .AnyAsync(e => e.Id == employerId, cancellationToken);

        if (!employerExists)
            throw new UnauthorizedAccessException("Current user is not an employer.");

        var now = DateTime.UtcNow;

        // ExpiryDate is on Request. Join request to get expiry.
        var accessRows = await (
            from request in _context.Requests.AsNoTracking()
            join worker in _context.Workers.AsNoTracking() on request.WorkerId equals worker.Id
            join permission in _context.Permissions.AsNoTracking() on request.Id equals permission.RequestId
            join workerInfo in _context.WorkerInfos.AsNoTracking() on permission.InfoId equals workerInfo.Id
            where request.EmployerId == employerId
                  && permission.Status == PermissionStatus.Approved
                  && request.ExpiryDate > now
            select new ActiveAccessRow
            {
                RequestId = request.Id,
                WorkerId = worker.Id,
                WorkerName = worker.Name,
                WorkerEmail = worker.Email,
                Reason = request.Reason,
                PermissionId = permission.Id,
                // Use CustomLabel for custom fields, Field label for preset fields
                DataType = workerInfo.CustomLabel ?? workerInfo.Field!.Label,
                Value = workerInfo.Value ?? string.Empty,
                GrantedAt = permission.LastUpdatedAt ?? DateTime.UtcNow,
                ExpiryDate = request.ExpiryDate
            }
        ).ToListAsync(cancellationToken);

        return accessRows
            .GroupBy(row => row.RequestId)
            .Select(group =>
            {
                var rows = group.ToList();
                return new EmployerActiveAccessDto
                {
                    RequestId = group.Key,
                    WorkerId = rows.First().WorkerId,
                    WorkerName = rows.First().WorkerName,
                    WorkerEmail = rows.First().WorkerEmail,
                    Reason = rows.First().Reason,
                    GrantedAt = rows.Max(r => r.GrantedAt),
                    ExpiryDate = rows.Select(r => r.ExpiryDate).Min(),
                    WorkerInfo = rows.Select(r => new EmployerActiveAccessInfoDto
                    {
                        PermissionId = r.PermissionId,
                        DataType = r.DataType,
                        Value = r.Value
                    }).ToList()
                };
            })
            .OrderByDescending(a => a.GrantedAt)
            .ToList();
    }

    private class ActiveAccessRow
    {
        public Guid RequestId { get; set; }
        public Guid WorkerId { get; set; }
        public string WorkerName { get; set; } = string.Empty;
        public string WorkerEmail { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public Guid PermissionId { get; set; }
        public string DataType { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public DateTime GrantedAt { get; set; }
        public DateTime ExpiryDate { get; set; }
    }
}
