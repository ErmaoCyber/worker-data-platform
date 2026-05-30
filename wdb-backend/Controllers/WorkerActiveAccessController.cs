using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using wdb_backend.Abstractions;

namespace wdb_backend.Controllers;

/// <summary>
/// Worker-side active access API.
/// Used by the worker Data Access page after requests are approved.
/// </summary>
[Authorize]
[ApiController]
[Route("api/worker/data-access")]
public class WorkerActiveAccessController : ControllerBase
{
    private readonly IActiveAccessService _activeAccessService;

    public WorkerActiveAccessController(IActiveAccessService activeAccessService)
    {
        _activeAccessService = activeAccessService;
    }

    private Guid GetCurrentWorkerId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)
                 ?? User.FindFirst("sub");

        if (claim == null)
            throw new UnauthorizedAccessException("User ID not found in token");

        return Guid.Parse(claim.Value);
    }

    /// <summary>
    /// Return active approved access records for the current worker.
    /// </summary>
    [HttpGet("active-access")]
    public async Task<IActionResult> GetActiveAccess(
        [FromQuery] string? company,
        [FromQuery] string? dataType)
    {
        try
        {
            var workerId = GetCurrentWorkerId();

            var result = await _activeAccessService.GetActiveAccessAsync(
                workerId,
                company,
                dataType);

            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    /// <summary>
    /// Revoke one active permission.
    /// </summary>
    [HttpPatch("active-access/{permissionId}/revoke")]
    public async Task<IActionResult> RevokePermission(
        Guid permissionId,
        CancellationToken cancellationToken)
    {
        try
        {
            var workerId = GetCurrentWorkerId();

            await _activeAccessService.RevokePermissionAsync(
                workerId,
                permissionId,
                cancellationToken);

            return Ok(new { message = "Access revoked." });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex) when (ex.Message == "PERMISSION_NOT_FOUND")
        {
            return NotFound(new { message = "Permission not found." });
        }
        catch (InvalidOperationException ex) when (ex.Message == "PERMISSION_NOT_APPROVED")
        {
            return Conflict(new { message = "Only approved permissions can be revoked." });
        }
        catch (InvalidOperationException ex) when (ex.Message == "REQUEST_EXPIRED")
        {
            return Conflict(new { message = "This access has already expired." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }
}
