using Microsoft.EntityFrameworkCore;
using wdb_backend.Abstractions;
using wdb_backend.Data;
using wdb_backend.DTOs;
using wdb_backend.Models;

namespace wdb_backend.Services;

public class NotificationRepoImpl : INotificationRepository
{
    private readonly AppDbContext _dbContext;

    public NotificationRepoImpl(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>Persist a notification row.</summary>
    public async Task AddAsync(Models.Notification notification, CancellationToken ct = default)
    {
        await _dbContext.Notifications.AddAsync(notification, ct);
        await _dbContext.SaveChangesAsync(ct);
    }

    /// <summary>Mark a notification as read.</summary>
    public async Task UpdateStatusAsync(Guid notificationId, CancellationToken ct = default)
    {
        var notification = await _dbContext.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId, ct);

        if (notification == null) return;

        notification.IsRead = true;
        await _dbContext.SaveChangesAsync(ct);
    }

    /// <summary>Get all notifications where the worker is the recipient.</summary>
    public async Task<List<Models.Notification>> GetAllByWorkerIdAsync(Guid workerId, CancellationToken ct = default)
    {
        return await _dbContext.Notifications
            .Where(n => n.RecipientWorkerId == workerId)
            .ToListAsync(ct);
    }

    public async Task<List<Models.Notification>> GetAllUnreadByWorkerIdAsync(Guid workerId, CancellationToken ct = default)
    {
        return await _dbContext.Notifications
            .Where(n => n.RecipientWorkerId == workerId && !n.IsRead)
            .ToListAsync(ct);
    }

    public async Task<List<Models.Notification>> GetAllReadByWorkerIdAsync(Guid workerId, CancellationToken ct = default)
    {
        return await _dbContext.Notifications
            .Where(n => n.RecipientWorkerId == workerId && n.IsRead)
            .ToListAsync(ct);
    }

    public async Task<Models.Notification?> GetByIdAsync(Guid notificationId, CancellationToken ct)
    {
        return await _dbContext.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId, ct);
    }

    /// <summary>
    /// Format a NotificationEvent into a human-readable DTO.
    /// Looks up employer name from DB; worker info description is carried on the event.
    /// </summary>
    public async Task<NotificationFormat> FormatNotification(NotificationEvent e, CancellationToken ct)
    {
        var employerName = (await _dbContext.Employers
            .FirstOrDefaultAsync(emp => emp.Id == e.EmployerId, ct))?.Name;

        var workerName = (await _dbContext.Workers
            .FirstOrDefaultAsync(w => w.Id == e.WorkerId, ct))?.Name;

        return new NotificationFormat(
            employerName,
            workerName,
            e.Type.ToString(),
            e.FieldLabel,   // replaces old WorkerInfoId lookup
            e.CreateAt
        );
    }

    /// <summary>Format a persisted Notification row into a display DTO.</summary>
    public async Task<NotificationFormatComponent> FormatNotificationPipeline(Models.Notification n, CancellationToken ct)
    {
        // Derive employer from the linked request
        var request = n.RequestId.HasValue
            ? await _dbContext.Requests.FirstOrDefaultAsync(r => r.Id == n.RequestId, ct)
            : null;

        var employerName = request != null
            ? (await _dbContext.Employers.FirstOrDefaultAsync(e => e.Id == request.EmployerId, ct))?.Name
            : null;

        var workerName = n.RecipientWorkerId.HasValue
            ? (await _dbContext.Workers.FirstOrDefaultAsync(w => w.Id == n.RecipientWorkerId, ct))?.Name
            : null;

        return new NotificationFormatComponent(
            n.Id,
            employerName,
            workerName,
            n.Type,
            null,   // field label not stored on notification row — Phase 2 TODO
            n.CreatedAt
        );
    }

    /// <summary>Single-query fetch + format for a worker's notifications.</summary>
    public async Task<IList<NotificationFormatComponent>> GetFormattedNotificationsAsync(
        Guid workerId, bool? isRead, CancellationToken ct)
    {
        var rows = await (
            from n in _dbContext.Notifications
            where n.RecipientWorkerId == workerId
                  && (!isRead.HasValue || n.IsRead == isRead.Value)
            join req in _dbContext.Requests on n.RequestId equals req.Id into rg
            from req in rg.DefaultIfEmpty()
            join emp in _dbContext.Employers on req.EmployerId equals emp.Id into eg
            from emp in eg.DefaultIfEmpty()
            orderby n.CreatedAt descending
            select new
            {
                n.Id,
                EmployerName = (string?)emp.Name,
                n.Type,
                n.CreatedAt
            }
        ).ToListAsync(ct);

        return rows.Select(r => new NotificationFormatComponent(
            r.Id,
            r.EmployerName,
            null,
            r.Type,
            null,
            r.CreatedAt
        )).ToList();
    }
}
