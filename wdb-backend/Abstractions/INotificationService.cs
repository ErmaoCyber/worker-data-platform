using wdb_backend.DTOs;

namespace wdb_backend.Abstractions;

public interface INotificationService
{
    Task<bool> UpdateStatus(
        Guid notificationId,
        CancellationToken ct = default);

    // Worker notification service methods
    Task<List<Models.Notification>> GetAllAsync(
        Guid workerId,
        CancellationToken ct = default);

    Task<List<Models.Notification>> GetUnreadAsync(
        Guid workerId,
        CancellationToken ct = default);

    Task<List<Models.Notification>> GetReadAsync(
        Guid workerId,
        CancellationToken ct = default);

    Task<IList<NotificationFormatComponent>> GetFormattedWorkerAsync(
        Guid workerId,
        bool? isRead,
        CancellationToken ct = default);

    // Employer notification service methods
    Task<List<Models.Notification>> GetAllEmployerAsync(
        Guid employerId,
        CancellationToken ct = default);

    Task<List<Models.Notification>> GetUnreadEmployerAsync(
        Guid employerId,
        CancellationToken ct = default);

    Task<List<Models.Notification>> GetReadEmployerAsync(
        Guid employerId,
        CancellationToken ct = default);

    Task<IList<NotificationFormatComponent>> GetFormattedEmployerAsync(
        Guid employerId,
        bool? isRead,
        CancellationToken ct = default);

    Task<IList<NotificationFormatComponent>> NotificationFormat(
        List<Models.Notification> notifications,
        CancellationToken ct = default);

    /// <summary>
    /// Legacy worker-only formatted query.
    /// Kept for compatibility with existing controller code.
    /// </summary>
    Task<IList<NotificationFormatComponent>> GetFormattedAsync(
        Guid workerId,
        bool? isRead,
        CancellationToken ct = default);
}
