using Microsoft.AspNetCore.Mvc;
using wdb_backend.Abstractions;
using wdb_backend.Models;

namespace wdb_backend.Controllers;

/// <summary>
/// Handles worker approve / reject actions on individual permissions.
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

    /// <summary>Approve a single permission.</summary>
    [HttpPatch("{permissionId}/approve")]
    public async Task<ActionResult<Permission>> ApprovePermission(
        Guid permissionId,
        CancellationToken cancellationToken)
    {
        try
        {
            var updated = await _permissionService.UpdateAsync(permissionId, 1, cancellationToken);
            return Ok(updated);
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

    /// <summary>Reject a single permission.</summary>
    [HttpPatch("{permissionId}/reject")]
    public async Task<ActionResult<Permission>> RejectPermission(
        Guid permissionId,
        CancellationToken cancellationToken)
    {
        try
        {
            var updated = await _permissionService.UpdateAsync(permissionId, 2, cancellationToken);
            return Ok(updated);
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
