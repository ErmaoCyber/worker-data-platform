using Microsoft.EntityFrameworkCore;
using wdb_backend.Abstractions;
using wdb_backend.Data;
using wdb_backend.DTOs;

namespace wdb_backend.Services;

public class NotificationRepoImpl : INotificationRepository
{
    private readonly AppDbContext _dbContext;

    public NotificationRepoImpl(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(Models.Notification notification, CancellationToken ct = default)
    {
        await _dbContext.Notifications.AddAsync(notification, ct);
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task UpdateStatusAsync(Guid notificationId, CancellationToken ct = default)
    {
        var notification = await _dbContext.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId, ct);

        if (notification == null) return;

        notification.IsRead = true;
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task<List<Models.Notification>> GetAllByWorkerIdAsync(Guid workerId, CancellationToken ct = default)
    {
        return await _dbContext.Notifications
            .Where(n => n.RecipientWorkerId == workerId)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<List<Models.Notification>> GetAllUnreadByWorkerIdAsync(Guid workerId, CancellationToken ct = default)
    {
        return await _dbContext.Notifications
            .Where(n => n.RecipientWorkerId == workerId && n.IsRead == false)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<List<Models.Notification>> GetAllReadByWorkerIdAsync(Guid workerId, CancellationToken ct = default)
    {
        return await _dbContext.Notifications
            .Where(n => n.RecipientWorkerId == workerId && n.IsRead == true)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<Models.Notification?> GetByIdAsync(Guid notificationId, CancellationToken ct)
    {
        return await _dbContext.Notifications.FirstOrDefaultAsync(n => n.Id == notificationId, ct);
    }

    public async Task<NotificationFormatComponent> FormatNotificationPipeline(
        Models.Notification n,
        CancellationToken ct = default)
    {
        // Resolve sender name (employer) via the associated request when present.
        // Worker-bound notifications (NEW_REQUEST / DATA_ACCESSED) have an employer sender.
        string? employerName = null;
        if (n.RequestId.HasValue)
        {
            employerName = await _dbContext.Requests
                .AsNoTracking()
                .Where(r => r.Id == n.RequestId.Value)
                .Join(_dbContext.Employers.AsNoTracking(),
                    r => r.EmployerId,
                    e => e.Id,
                    (r, e) => e.Name)
                .FirstOrDefaultAsync(ct);
        }

        return new NotificationFormatComponent(
            n.Id,
            employerName,
            null,        // worker name: not relevant when the worker is the recipient
            n.Type,
            null,        // legacy per-field description no longer applies
            n.CreatedAt);
    }

    public async Task<IList<NotificationFormatComponent>> GetFormattedNotificationsAsync(
        Guid workerId,
        bool? isRead,
        CancellationToken ct = default)
    {
        var rows = await (
            from n in _dbContext.Notifications.AsNoTracking()
            where n.RecipientWorkerId == workerId
                  && (!isRead.HasValue || n.IsRead == isRead.Value)
            join r in _dbContext.Requests.AsNoTracking() on n.RequestId equals r.Id into rg
            from r in rg.DefaultIfEmpty()
            join e in _dbContext.Employers.AsNoTracking() on (r != null ? r.EmployerId : Guid.Empty) equals e.Id into eg
            from e in eg.DefaultIfEmpty()
            orderby n.CreatedAt descending
            select new
            {
                n.Id,
                EmployerName = e != null ? (string?)e.Name : null,
                n.Type,
                n.CreatedAt
            }
        ).ToListAsync(ct);

        return rows.Select(row => new NotificationFormatComponent(
            row.Id,
            row.EmployerName,
            null,
            row.Type,
            null,
            row.CreatedAt
        )).ToList();
    }
}
