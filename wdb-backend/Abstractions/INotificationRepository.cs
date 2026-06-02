using wdb_backend.DTOs;

namespace wdb_backend.Abstractions;

public interface INotificationRepository
{
    Task AddAsync(Models.Notification notification, CancellationToken ct = default);

    Task UpdateStatusAsync(Guid notificationId, CancellationToken ct = default);

    Task<Models.Notification?> GetByIdAsync(
        Guid notificationId,
        CancellationToken ct = default);

    // Worker recipient queries
    Task<List<Models.Notification>> GetAllByWorkerIdAsync(
        Guid workerId,
        CancellationToken ct = default);

    Task<List<Models.Notification>> GetAllUnreadByWorkerIdAsync(
        Guid workerId,
        CancellationToken ct = default);

    Task<List<Models.Notification>> GetAllReadByWorkerIdAsync(
        Guid workerId,
        CancellationToken ct = default);

    Task<IList<NotificationFormatComponent>> GetFormattedWorkerNotificationsAsync(
        Guid workerId,
        bool? isRead,
        CancellationToken ct = default);

    // Employer recipient queries
    Task<List<Models.Notification>> GetAllByEmployerIdAsync(
        Guid employerId,
        CancellationToken ct = default);

    Task<List<Models.Notification>> GetAllUnreadByEmployerIdAsync(
        Guid employerId,
        CancellationToken ct = default);

    Task<List<Models.Notification>> GetAllReadByEmployerIdAsync(
        Guid employerId,
        CancellationToken ct = default);

    Task<IList<NotificationFormatComponent>> GetFormattedEmployerNotificationsAsync(
        Guid employerId,
        bool? isRead,
        CancellationToken ct = default);

    // Existing formatting helpers
    Task<NotificationFormat> FormatNotification(
        NotificationEvent e,
        CancellationToken ct = default);

    Task<NotificationFormatComponent> FormatNotificationPipeline(
        Models.Notification n,
        CancellationToken ct = default);

    /// <summary>
    /// Legacy worker-only formatted query.
    /// Kept for compatibility with existing code.
    /// </summary>
    Task<IList<NotificationFormatComponent>> GetFormattedNotificationsAsync(
        Guid workerId,
        bool? isRead,
        CancellationToken ct = default);
}
