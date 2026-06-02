using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
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

    public NotificationController(
        IMediator mediator,
        INotificationService notificationService)
    {
        _mediator = mediator;
        _notificationService = notificationService;
    }

    private Guid GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)
                 ?? User.FindFirst("sub");

        if (claim == null)
            throw new UnauthorizedAccessException("User ID not found in token.");

        return Guid.Parse(claim.Value);
    }

    /// <summary>
    /// Get notifications for the current worker.
    /// Example:
    /// GET /api/notification/worker/me
    /// GET /api/notification/worker/me?isRead=false
    /// </summary>
    [HttpGet("worker/me")]
    public async Task<ActionResult> GetMyWorkerNotifications(
        [FromQuery] bool? isRead,
        CancellationToken ct)
    {
        try
        {
            var workerId = GetCurrentUserId();

            var result = await _notificationService.GetFormattedWorkerAsync(
                workerId,
                isRead,
                ct);

            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Get notifications for the current employer.
    /// Example:
    /// GET /api/notification/employer/me
    /// GET /api/notification/employer/me?isRead=false
    /// </summary>
    [HttpGet("employer/me")]
    public async Task<ActionResult> GetMyEmployerNotifications(
        [FromQuery] bool? isRead,
        CancellationToken ct)
    {
        try
        {
            var employerId = GetCurrentUserId();

            var result = await _notificationService.GetFormattedEmployerAsync(
                employerId,
                isRead,
                ct);

            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Mark a notification as read.
    /// </summary>
    [HttpPatch("{notificationId}/read")]
    public async Task<IActionResult> MarkAsRead(
        Guid notificationId,
        CancellationToken ct)
    {
        var success = await _notificationService.UpdateStatus(notificationId, ct);

        if (!success)
            return NotFound(new { message = "Notification not found." });

        return Ok(new { message = "Notification marked as read." });
    }

    // ── Legacy endpoints kept for compatibility ────────────────────────────

    /// <summary>
    /// Legacy mark-as-read endpoint.
    /// </summary>
    [HttpPatch("{notificationId}")]
    public async Task<IActionResult> CheckNotification(
        Guid notificationId,
        CancellationToken ct)
    {
        var success = await _notificationService.UpdateStatus(notificationId, ct);

        if (!success)
            return NotFound(new { message = "Notification not found." });

        return Ok(new { message = "already read" });
    }

    /// <summary>
    /// Legacy worker notification endpoint.
    /// Prefer GET /api/notification/worker/me.
    /// </summary>
    [HttpGet("all/{workerId}")]
    public async Task<ActionResult> GetAllNotifications(
        Guid workerId,
        CancellationToken ct)
    {
        var result = await _notificationService.GetFormattedAsync(
            workerId,
            null,
            ct);

        return Ok(result);
    }

    /// <summary>
    /// Legacy worker unread notification endpoint.
    /// Prefer GET /api/notification/worker/me?isRead=false.
    /// </summary>
    [HttpGet("unread/{workerId}")]
    public async Task<ActionResult> GetUnreadNotifications(
        Guid workerId,
        CancellationToken ct)
    {
        var result = await _notificationService.GetFormattedAsync(
            workerId,
            false,
            ct);

        return Ok(ApiResponse<IList<NotificationFormatComponent>>.Ok(result));
    }
}
