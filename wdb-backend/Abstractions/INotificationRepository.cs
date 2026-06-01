using wdb_backend.DTOs;

namespace wdb_backend.Abstractions;

public interface INotificationRepository
{
    // Persist a notification row.
    Task AddAsync(Models.Notification notification, CancellationToken ct = default);

    // Flip is_read to true.
    Task UpdateStatusAsync(Guid notificationId, CancellationToken ct = default);

    // Worker-recipient lookups (recipient_worker_id).
    Task<List<Models.Notification>> GetAllByWorkerIdAsync(Guid workerId, CancellationToken ct = default);
    Task<List<Models.Notification>> GetAllUnreadByWorkerIdAsync(Guid workerId, CancellationToken ct = default);
    Task<List<Models.Notification>> GetAllReadByWorkerIdAsync(Guid workerId, CancellationToken ct = default);

    Task<Models.Notification?> GetByIdAsync(Guid notificationId, CancellationToken ct);

    // Per-row format used by the worker bell. Joins request -> employer to fill the sender name.
    Task<NotificationFormatComponent> FormatNotificationPipeline(Models.Notification n, CancellationToken ct = default);

    // Single JOIN query: isRead=null returns all, false returns unread, true returns read.
    Task<IList<NotificationFormatComponent>> GetFormattedNotificationsAsync(
        Guid workerId,
        bool? isRead,
        CancellationToken ct = default);
}
