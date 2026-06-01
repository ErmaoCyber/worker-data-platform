using wdb_backend.DTOs;

namespace wdb_backend.Abstractions;

public interface INotificationService
{
    // Create a notification, persist it, and push it to the recipient's SignalR group.
    // Exactly one of recipientWorkerId / recipientEmployerId must be set.
    Task NotifyAsync(
        string type,
        Guid? recipientWorkerId,
        Guid? recipientEmployerId,
        Guid? requestId,
        CancellationToken ct = default);

    Task<bool> UpdateStatus(Guid notificationId, CancellationToken ct);

    Task<List<Models.Notification>> GetAllAsync(Guid workerId, CancellationToken ct);
    Task<List<Models.Notification>> GetUnreadAsync(Guid workerId, CancellationToken ct);
    Task<List<Models.Notification>> GetReadAsync(Guid workerId, CancellationToken ct);

    // Pipeline that turns raw notification rows into readable components.
    Task<IList<NotificationFormatComponent>> NotificationFormat(
        List<Models.Notification> notifications,
        CancellationToken ct);

    // Single-query version: isRead=null returns all, false unread, true read.
    Task<IList<NotificationFormatComponent>> GetFormattedAsync(
        Guid workerId,
        bool? isRead,
        CancellationToken ct);
}
