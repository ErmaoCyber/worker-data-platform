using Microsoft.AspNetCore.Mvc;
using wdb_backend.Abstractions;
using wdb_backend.Models;

namespace wdb_backend.Controllers;

/// <summary>
/// API controller for managing permission approval and rejection.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class PermissionController : ControllerBase
{
    private readonly IPermissionService _permissionService;

    public PermissionController(IPermissionService permissionService)
    {
        _permissionService = permissionService;
    }

    [HttpPatch("{permissionId}/approve")]
    public async Task<ActionResult<Permission>> ApprovePermission(
        Guid permissionId,
        [FromBody] ApprovePermissionRequest request,
        CancellationToken cancellationToken)
    {
        if (request.ExpiryDate == null)
        {
            return BadRequest(new { error = "EXPIRY_DATE_REQUIRED" });
        }

        try
        {
            var update = await _permissionService.UpdateAsync(
                permissionId,
                1,
                request.ExpiryDate,
                cancellationToken
            );

            return Ok(update);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { error = "PERMISSION_NOT_FOUND" });
        }
        catch (InvalidOperationException)
        {
            return UnprocessableEntity(new { error = "INVALID_STATUS_CHANGE" });
        }
    }

    [HttpPatch("{permissionId}/reject")]
    public async Task<ActionResult<Permission>> RejectPermission(
        Guid permissionId,
        CancellationToken cancellationToken)
    {
        try
        {
            var update = await _permissionService.UpdateAsync(
                permissionId,
                2,
                null,
                cancellationToken
            );

            return Ok(update);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { error = "PERMISSION_NOT_FOUND" });
        }
        catch (InvalidOperationException)
        {
            return UnprocessableEntity(new { error = "INVALID_STATUS_CHANGE" });
        }
    }
}

public class ApprovePermissionRequest
{
    public DateTime? ExpiryDate { get; set; }
}
