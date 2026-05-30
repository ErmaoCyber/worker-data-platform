using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using wdb_backend.Abstractions;
using wdb_backend.Common;
using wdb_backend.DTOs;

namespace wdb_backend.Controllers;

[Authorize]
[ApiController]
[Route("api/notification")]
public class NotificationController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly INotificationService _notificationService;

    public NotificationController(IMediator mediator, INotificationService notificationService)
    {
        _mediator = mediator;
        _notificationService = notificationService;
    }

    /// <summary>Send an "access" notification to a worker.</summary>
    [HttpPost("access")]
    public async Task<IActionResult> AccessInfo([FromBody] NotificationInfo notiInfo, CancellationToken ct)
    {
        await _mediator.Send(
            new NotificationCommand(notiInfo.EmployerId, notiInfo.WorkerId, notiInfo.RequestId, null, NotificationType.Access),
            ct);
        return Ok(new { message = "already notified" });
    }

    /// <summary>Send a "request" notification to a worker.</summary>
    [HttpPost("request")]
    public async Task<IActionResult> RequestInfo([FromBody] NotificationInfo notiInfo, CancellationToken ct)
    {
        await _mediator.Send(
            new NotificationCommand(notiInfo.EmployerId, notiInfo.WorkerId, notiInfo.RequestId, null, NotificationType.Request),
            ct);
        return Ok(new { message = "already notified" });
    }

    /// <summary>Mark a notification as read.</summary>
    [HttpPatch("{notificationId}")]
    public async Task<IActionResult> CheckNotification(Guid notificationId, CancellationToken ct)
    {
        var success = await _notificationService.UpdateStatus(notificationId, ct);
        if (!success) return NotFound(new { message = "Notification not found" });
        return Ok(new { message = "already read" });
    }

    /// <summary>Get all notifications for a worker.</summary>
    [HttpGet("all/{workerId}")]
    public async Task<ActionResult> GetAllNotifications(Guid workerId, CancellationToken ct)
    {
        var result = await _notificationService.GetFormattedAsync(workerId, null, ct);
        return Ok(result);
    }

    /// <summary>Get unread notifications for a worker.</summary>
    [HttpGet("unread/{workerId}")]
    public async Task<ActionResult> GetUnreadNotifications(Guid workerId, CancellationToken ct)
    {
        var result = await _notificationService.GetFormattedAsync(workerId, false, ct);
        return Ok(result);
    }
}
