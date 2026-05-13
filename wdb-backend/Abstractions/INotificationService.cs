using wdb_backend.DTOs;

namespace wdb_backend.Abstractions;

public interface INotificationService
{
    Task<bool> UpdateStatus(Guid notificationId, CancellationToken ct);

    Task<List<Models.Notification>> GetAllAsync(Guid workerId, CancellationToken ct);
    Task<List<Models.Notification>> GetUnreadAsync(Guid workerId, CancellationToken ct);
    Task<List<Models.Notification>> GetReadAsync(Guid workerId, CancellationToken ct);

    // pipeline to handle all the notification object to the readable object
    Task<IList<NotificationFormatComponent>> NotificationFormat(List<Models.Notification> notifications, CancellationToken ct);
}
