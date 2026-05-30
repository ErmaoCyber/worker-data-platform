using wdb_backend.DTOs;

namespace wdb_backend.Abstractions;

public interface INotificationRepository
{
    Task AddAsync(Models.Notification notification, CancellationToken ct = default);
    Task UpdateStatusAsync(Guid notificationId, CancellationToken ct = default);
    Task<List<Models.Notification>> GetAllByWorkerIdAsync(Guid workerId, CancellationToken ct = default);
    Task<List<Models.Notification>> GetAllUnreadByWorkerIdAsync(Guid workerId, CancellationToken ct = default);
    Task<List<Models.Notification>> GetAllReadByWorkerIdAsync(Guid workerId, CancellationToken ct = default);
    Task<Models.Notification?> GetByIdAsync(Guid notificationId, CancellationToken ct);
    Task<NotificationFormat> FormatNotification(NotificationEvent e, CancellationToken ct = default);
    Task<NotificationFormatComponent> FormatNotificationPipeline(Models.Notification n, CancellationToken ct = default);
    Task<IList<NotificationFormatComponent>> GetFormattedNotificationsAsync(Guid workerId, bool? isRead, CancellationToken ct = default);
}
