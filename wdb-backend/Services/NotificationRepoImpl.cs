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

    public async Task AddAsync(Models.Notification notification, CancellationToken ct = default)
    {
        await _dbContext.Notifications.AddAsync(notification, ct);
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task UpdateStatusAsync(Guid notificationId, CancellationToken ct = default)
    {
        var notification = await _dbContext.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId, ct);

        if (notification == null) return;  // let controller handle exception

        notification.IsRead = true;
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task<List<Models.Notification>> GetAllByWorkerIdAsync(Guid workerId, CancellationToken ct = default)
    {
        return await _dbContext.Notifications.Where(notification => notification.WorkerId == workerId).ToListAsync(ct);
    }

    public async Task<List<Models.Notification>> GetAllUnreadByWorkerIdAsync(Guid workerId, CancellationToken ct = default)
    {
        return await _dbContext.Notifications.Where(notification => notification.WorkerId == workerId && notification.IsRead == false).ToListAsync(ct);
    }

    public async Task<List<Models.Notification>> GetAllReadByWorkerIdAsync(Guid workerId, CancellationToken ct = default)
    {
        return await _dbContext.Notifications.Where(notification => notification.WorkerId == workerId && notification.IsRead == true).ToListAsync(ct);
    }

    public async Task<Models.Notification?> GetByIdAsync(Guid notificationId, CancellationToken ct)
    {
        return await _dbContext.Notifications.FirstOrDefaultAsync(notification => notification.Id == notificationId, ct);
    }

    // customized method for showing the readable data from the frontend
    public async Task<NotificationFormat> FormatNotification(NotificationEvent e, CancellationToken ct)
    {
        var employerName = (await _dbContext.Employers.FirstOrDefaultAsync(employer => employer.Id == e.EmployerId, ct))?.Name;
        var workerName = (await _dbContext.Workers.FirstOrDefaultAsync(worker => worker.Id == e.WorkerId, ct))?.Name;
        var workerInfoDesc = (await _dbContext.WorkerInfos.FirstOrDefaultAsync(workerInfo => workerInfo.Id == e.WorkerInfoId, ct))?.Desc;
        return new NotificationFormat
        (
            employerName,
            workerName,
            e.Type.ToString(),
            workerInfoDesc,
            e.CreateAt
        );
    }

    public async Task<NotificationFormatComponent> FormatNotificationPipeline(Models.Notification n, CancellationToken ct)
    {
        var employerName = (await _dbContext.Employers.FirstOrDefaultAsync(employer => employer.Id == n.EmployerId, ct))?.Name;
        var workerName = (await _dbContext.Workers.FirstOrDefaultAsync(worker => worker.Id == n.WorkerId, ct))?.Name;
        var workerInfoDesc = (await _dbContext.WorkerInfos.FirstOrDefaultAsync(workerInfo => workerInfo.Id == n.WorkerInfoId, ct))?.Desc;
        return new NotificationFormatComponent
        (
            n.Id,
            employerName,
            workerName,
            n.Type,
            workerInfoDesc,
            n.CreateAt
        );
    }

    public async Task<IList<NotificationFormatComponent>> GetFormattedNotificationsAsync(
        Guid workerId, bool? isRead, CancellationToken ct)
    {
        var rows = await (
            from n in _dbContext.Notifications
            where n.WorkerId == workerId && (!isRead.HasValue || n.IsRead == isRead.Value)
            join e in _dbContext.Employers on n.EmployerId equals e.Id into eg
            from e in eg.DefaultIfEmpty()
            join wi in _dbContext.WorkerInfos on n.WorkerInfoId equals wi.Id into wig
            from wi in wig.DefaultIfEmpty()
            orderby n.CreateAt descending
            select new
            {
                n.Id,
                EmployerName = (string?)e.Name,
                n.Type,
                WorkerInfoDesc = (string?)wi.Desc,
                n.CreateAt
            }
        ).ToListAsync(ct);

        return rows.Select(r => new NotificationFormatComponent(
            r.Id,
            r.EmployerName,
            null,
            r.Type,
            r.WorkerInfoDesc,
            r.CreateAt
        )).ToList();
    }
}
