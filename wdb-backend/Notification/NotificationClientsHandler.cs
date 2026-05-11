using MediatR;
using Microsoft.AspNetCore.SignalR;
using wdb_backend.Abstractions;
using wdb_backend.DTOs;

namespace wdb_backend.Notification;

public class NotificationClientsHandler : INotificationHandler<NotificationEvent>
{
    private readonly IHubContext<NotificationsHub> _hubContext;
    private readonly INotificationRepository _notificationRepo;

    // inject IHubContext to send the message to the client; inject notificationRepo to save notification to the database
    public NotificationClientsHandler(IHubContext<NotificationsHub> hubContext, INotificationRepository notificationRepo)
    {
        _hubContext = hubContext;
        _notificationRepo = notificationRepo;
    }

    public async Task Handle(NotificationEvent e, CancellationToken ct)
    {
        // save the notification info to the database before notify (the field - is_read is default false)
        await _notificationRepo.AddAsync(new Models.Notification
        {
            EmployerId = e.EmployerId,
            WorkerId = e.WorkerId,
            WorkerInfoId = e.WorkerInfoId,
            Type = e.Type.ToString(),
            CreateAt = e.CreateAt,
            IsRead = false
        }, ct);

        // format the notification
        var format = await _notificationRepo.FormatNotification(e, ct);

        // send to specific worker only - to the method of NotificationInfo
        // await _hubContext.Clients.Group(e.WorkerId.ToString()).SendAsync("NotificationInfo", $"{e.EmployerId}-{e.WorkerId}-{e.WorkerInfoId}-{e.Type}-{e.CreateAt}", ct);
        await _hubContext.Clients.Group(e.WorkerId.ToString()).SendAsync("NotificationInfo", $"{format.EmployerName} {format.NotificationType.ToLower()}ed your {format.WorkInfoDesc} at {format.NotificationTime}", ct);
    }
}
