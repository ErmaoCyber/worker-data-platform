using wdb_backend.DTOs;

namespace wdb_backend.Abstractions;
using wdb_backend.Models;

public interface INotificationRepository
{
    // save notification to the database
    Task AddAsync(Notification notification, CancellationToken ct = default);

    // update the is_read status of notification after the user reading the notification
    Task UpdateStatusAsync(Guid notificationId, CancellationToken ct = default);

    // get all the notifications based on userId
    Task<List<Notification>> GetAllByWorkerIdAsync(Guid workerId, CancellationToken ct = default);

    // get all the unread notification based on userId
    Task<List<Notification>> GetAllUnreadByWorkerIdAsync(Guid workerId, CancellationToken ct = default);

    // get all the read notification based on userId
    Task<List<Notification>> GetAllReadByWorkerIdAsync(Guid workId, CancellationToken ct = default);
    Task<Notification?> GetByIdAsync(Guid notificationId, CancellationToken ct);
    // customized method for showing the readable data from the frontend
    Task<NotificationFormat> FormatNotification(NotificationEvent e, CancellationToken ct = default);
    // different parameter from the FormatNotification, could merge to be one method using
    Task<NotificationFormatComponent> FormatNotificationPipeline(Notification n, CancellationToken ct = default);
}
