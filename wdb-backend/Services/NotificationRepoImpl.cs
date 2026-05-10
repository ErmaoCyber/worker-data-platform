using Microsoft.EntityFrameworkCore;
using wdb_backend.Abstractions;
using wdb_backend.Data;
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
}
