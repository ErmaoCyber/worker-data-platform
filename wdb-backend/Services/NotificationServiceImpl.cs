using Microsoft.AspNetCore.SignalR;
using wdb_backend.Abstractions;
using wdb_backend.DTOs;
using wdb_backend.Notification;

namespace wdb_backend.Services;

public class NotificationServiceImpl : INotificationService
{
    private readonly INotificationRepository _notificationRepo;
    private readonly IHubContext<NotificationsHub> _hubContext;

    public NotificationServiceImpl(
        INotificationRepository notificationRepo,
        IHubContext<NotificationsHub> hubContext)
    {
        _notificationRepo = notificationRepo;
        _hubContext = hubContext;
    }

    public async Task NotifyAsync(
        string type,
        Guid? recipientWorkerId,
        Guid? recipientEmployerId,
        Guid? requestId,
        CancellationToken ct = default)
    {
        // Mirror the database CHECK constraint: exactly one recipient must be set.
        if (recipientWorkerId.HasValue == recipientEmployerId.HasValue)
        {
            throw new ArgumentException(
                "Exactly one of recipientWorkerId / recipientEmployerId must be provided.");
        }

        var notification = new Models.Notification
        {
            Type = type,
            RecipientWorkerId = recipientWorkerId,
            RecipientEmployerId = recipientEmployerId,
            RequestId = requestId,
            IsRead = false
        };

        await _notificationRepo.AddAsync(notification, ct);

        // Push to the recipient's SignalR group. The hub joins clients to a group
        // named after their user id (worker GUID string) on connect.
        var groupId = (recipientWorkerId ?? recipientEmployerId)!.Value.ToString();
        await _hubContext.Clients.Group(groupId).SendAsync(
            "notification",
            new
            {
                id = notification.Id,
                type,
                requestId,
                createdAt = notification.CreatedAt
            },
            ct);
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

    public async Task<IList<NotificationFormatComponent>> NotificationFormat(
        List<Models.Notification> notifications,
        CancellationToken ct)
    {
        var list = new List<NotificationFormatComponent>();
        foreach (var n in notifications)
        {
            list.Add(await _notificationRepo.FormatNotificationPipeline(n, ct));
        }
        return list;
    }

    public async Task<IList<NotificationFormatComponent>> GetFormattedAsync(
        Guid workerId,
        bool? isRead,
        CancellationToken ct)
    {
        return await _notificationRepo.GetFormattedNotificationsAsync(workerId, isRead, ct);
    }
}
