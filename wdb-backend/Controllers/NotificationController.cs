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
    private readonly INotificationService _notificationService;

    public NotificationController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    /*
    // Demo trigger endpoints from the old design (frontend would POST here to fire a notification).
    // Notifications are now produced server-side by business actions
    // (employer creating a request, employer viewing data, etc.), so these manual
    // trigger endpoints are disabled. Kept here for reference.
    [HttpPost("access")]
    public async Task<IActionResult> AccessInfo([FromBody] NotificationInfo notiInfo, CancellationToken ct)
    {
        await _mediator.Send(new NotificationCommand(notiInfo.EmployerId, notiInfo.WorkerId, notiInfo.WorkerInfoId, LegacyNotificationType.Access), ct);
        return Ok(new { message = "already notified" });
    }

    [HttpPost("request")]
    public async Task<IActionResult> RequestInfo([FromBody] NotificationInfo notiInfo, CancellationToken ct)
    {
        await _mediator.Send(new NotificationCommand(notiInfo.EmployerId, notiInfo.WorkerId, notiInfo.WorkerInfoId, LegacyNotificationType.Request), ct);
        return Ok(new { message = "already notified" });
    }
    */

    // Mark a notification as read.
    [HttpPatch("{notificationId}")]
    public async Task<IActionResult> CheckNotification(Guid notificationId, CancellationToken ct)
    {
        var success = await _notificationService.UpdateStatus(notificationId, ct);
        if (!success) return NotFound(new { message = "Notification not found" });
        return Ok(new { message = "already read" });
    }

    // All notifications for a worker.
    [HttpGet("all/{workerId}")]
    public async Task<ActionResult<ApiResponse<IList<NotificationFormatComponent>>>> GetAll(
        Guid workerId,
        CancellationToken ct)
    {
        var notificationList = await _notificationService.GetFormattedAsync(workerId, null, ct);
        return Ok(ApiResponse<IList<NotificationFormatComponent>>.Ok(notificationList, "OK"));
    }

    // Unread notifications for a worker.
    [HttpGet("unread/{workerId}")]
    public async Task<IActionResult> GetUnread(Guid workerId, CancellationToken ct)
    {
        var notificationList = await _notificationService.GetFormattedAsync(workerId, false, ct);
        return Ok(ApiResponse<IList<NotificationFormatComponent>>.Ok(notificationList, "OK"));
    }

    // Read notifications for a worker.
    [HttpGet("read/{workerId}")]
    public async Task<IActionResult> GetRead(Guid workerId, CancellationToken ct)
    {
        var notificationList = await _notificationService.GetFormattedAsync(workerId, true, ct);
        return Ok(ApiResponse<IList<NotificationFormatComponent>>.Ok(notificationList, "OK"));
    }
}
