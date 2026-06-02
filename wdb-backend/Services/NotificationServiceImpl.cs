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

    public async Task<bool> UpdateStatus(
        Guid notificationId,
        CancellationToken ct = default)
    {
        var notification = await _notificationRepo.GetByIdAsync(notificationId, ct);

        if (notification == null)
            return false;

        await _notificationRepo.UpdateStatusAsync(notificationId, ct);
        return true;
    }

    // ── Worker recipient methods ───────────────────────────────────────────

    public async Task<List<Models.Notification>> GetAllAsync(
        Guid workerId,
        CancellationToken ct = default)
    {
        return await _notificationRepo.GetAllByWorkerIdAsync(workerId, ct);
    }

    public async Task<List<Models.Notification>> GetUnreadAsync(
        Guid workerId,
        CancellationToken ct = default)
    {
        return await _notificationRepo.GetAllUnreadByWorkerIdAsync(workerId, ct);
    }

    public async Task<List<Models.Notification>> GetReadAsync(
        Guid workerId,
        CancellationToken ct = default)
    {
        return await _notificationRepo.GetAllReadByWorkerIdAsync(workerId, ct);
    }

    public async Task<IList<NotificationFormatComponent>> GetFormattedWorkerAsync(
        Guid workerId,
        bool? isRead,
        CancellationToken ct = default)
    {
        return await _notificationRepo.GetFormattedWorkerNotificationsAsync(
            workerId,
            isRead,
            ct);
    }

    // ── Employer recipient methods ─────────────────────────────────────────

    public async Task<List<Models.Notification>> GetAllEmployerAsync(
        Guid employerId,
        CancellationToken ct = default)
    {
        return await _notificationRepo.GetAllByEmployerIdAsync(employerId, ct);
    }

    public async Task<List<Models.Notification>> GetUnreadEmployerAsync(
        Guid employerId,
        CancellationToken ct = default)
    {
        return await _notificationRepo.GetAllUnreadByEmployerIdAsync(employerId, ct);
    }

    public async Task<List<Models.Notification>> GetReadEmployerAsync(
        Guid employerId,
        CancellationToken ct = default)
    {
        return await _notificationRepo.GetAllReadByEmployerIdAsync(employerId, ct);
    }

    public async Task<IList<NotificationFormatComponent>> GetFormattedEmployerAsync(
        Guid employerId,
        bool? isRead,
        CancellationToken ct = default)
    {
        return await _notificationRepo.GetFormattedEmployerNotificationsAsync(
            employerId,
            isRead,
            ct);
    }

    // ── Existing formatting helpers ────────────────────────────────────────

    public async Task<IList<NotificationFormatComponent>> NotificationFormat(
        List<Models.Notification> notifications,
        CancellationToken ct = default)
    {
        IList<NotificationFormatComponent> notificationList =
            new List<NotificationFormatComponent>();

        foreach (var notification in notifications)
        {
            notificationList.Add(
                await _notificationRepo.FormatNotificationPipeline(notification, ct));
        }

        return notificationList;
    }

    /// <summary>
    /// Legacy worker-only formatted query.
    /// </summary>
    public async Task<IList<NotificationFormatComponent>> GetFormattedAsync(
        Guid workerId,
        bool? isRead,
        CancellationToken ct = default)
    {
        return await _notificationRepo.GetFormattedNotificationsAsync(
            workerId,
            isRead,
            ct);
    }
}
