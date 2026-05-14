using wdb_backend.Abstractions;
using wdb_backend.DTOs;

namespace wdb_backend.Services;

public class NotificationServiceImpl : INotificationService
{
    private readonly INotificationRepository _notificationRepo;

    public NotificationServiceImpl(INotificationRepository notificationRepo)
    {
        _notificationRepo = notificationRepo;
    }

    public async Task<bool> UpdateStatus(Guid notificationId, CancellationToken ct)
    {
        var notification = await _notificationRepo.GetByIdAsync(notificationId, ct);
        if (notification == null) return false;

        await _notificationRepo.UpdateStatusAsync(notificationId, ct);
        return true;
    }

    public async Task<List<Models.Notification>> GetAllAsync(Guid workerId, CancellationToken ct)
    {
        return await _notificationRepo.GetAllByWorkerIdAsync(workerId, ct);
    }

    public async Task<List<Models.Notification>> GetUnreadAsync(Guid workerId, CancellationToken ct)
    {
        return await _notificationRepo.GetAllUnreadByWorkerIdAsync(workerId, ct);
    }

    public async Task<List<Models.Notification>> GetReadAsync(Guid workerId, CancellationToken ct)
    {
        return await _notificationRepo.GetAllReadByWorkerIdAsync(workerId, ct);
    }

    public async Task<IList<NotificationFormatComponent>> NotificationFormat(List<Models.Notification> notifications, CancellationToken ct)
    {
        IList<NotificationFormatComponent> notificationList = new List<NotificationFormatComponent>();
        foreach (var notification in notifications)
        {
            notificationList.Add(await _notificationRepo.FormatNotificationPipeline(notification, ct));
        }

        return notificationList;
    }

    public async Task<IList<NotificationFormatComponent>> GetFormattedAsync(Guid workerId, bool? isRead, CancellationToken ct)
    {
        return await _notificationRepo.GetFormattedNotificationsAsync(workerId, isRead, ct);
    }
}
