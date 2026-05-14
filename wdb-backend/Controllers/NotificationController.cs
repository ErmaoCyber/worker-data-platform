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
    // inject IMediator instance for decoupling
    private readonly IMediator _mediator;

    // inject notification service instance
    private readonly INotificationService _notificationService;

    public NotificationController(IMediator mediator, INotificationService notificationService)
    {
        _mediator = mediator;
        _notificationService = notificationService;
    }

    // sent command to CommandHandler
    [HttpPost("access")]
    public async Task<IActionResult> AccessInfo([FromBody] NotificationInfo notiInfo, CancellationToken ct)
    {
        // send the command to handler
        await _mediator.Send(new NotificationCommand(notiInfo.EmployerId, notiInfo.WorkerId, notiInfo.WorkerInfoId, NotificationType.Access), ct);
        return Ok(new { message = "already notified" });
    }

    [HttpPost("request")]
    public async Task<IActionResult> RequestInfo([FromBody] NotificationInfo notiInfo, CancellationToken ct)
    {
        await _mediator.Send(new NotificationCommand(notiInfo.EmployerId, notiInfo.WorkerId, notiInfo.WorkerInfoId, NotificationType.Request), ct);
        return Ok(new { message = "already notified" });
    }

    // when click the specific notification
    [HttpPatch("{notificationId}")]
    public async Task<IActionResult> CheckNotification(Guid notificationId, CancellationToken ct)
    {
        var success = await _notificationService.UpdateStatus(notificationId, ct);
        if (!success) return NotFound(new { message = "Notification not found" });
        return Ok(new { message = "already read" });
    }

    // get all the notifications
    [HttpGet("all/{workerId}")]
    public async Task<ActionResult<ApiResponse<IList<NotificationFormatComponent>>>> GetAll(Guid workerId, CancellationToken ct)
    {
        var notificationList = await _notificationService.GetFormattedAsync(workerId, null, ct);
        return Ok(ApiResponse<IList<NotificationFormatComponent>>.Ok(notificationList, "OK"));
    }

    // get all the unread notifications
    [HttpGet("unread/{workerId}")]
    public async Task<IActionResult> GetUnread(Guid workerId, CancellationToken ct)
    {
        var notificationList = await _notificationService.GetFormattedAsync(workerId, false, ct);
        return Ok(ApiResponse<IList<NotificationFormatComponent>>.Ok(notificationList, "OK"));
    }

    // get all the read notifications
    [HttpGet("read/{workerId}")]
    public async Task<IActionResult> GetRead(Guid workerId, CancellationToken ct)
    {
        var notificationList = await _notificationService.GetFormattedAsync(workerId, true, ct);
        return Ok(ApiResponse<IList<NotificationFormatComponent>>.Ok(notificationList, "OK"));
    }

}
