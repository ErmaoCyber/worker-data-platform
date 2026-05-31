using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using wdb_backend.Abstractions;
using wdb_backend.DTOs;

namespace wdb_backend.Controllers;

[ApiController]
[Route("api/Employer/active-access")]
public class EmployerActiveAccessController : ControllerBase
{
    private readonly IEmployerActiveAccessService _employerActiveAccessService;

    public EmployerActiveAccessController(
        IEmployerActiveAccessService employerActiveAccessService)
    {
        _employerActiveAccessService = employerActiveAccessService;
    }

    /// <summary>
    /// Get all active worker data access granted to the current employer.
    /// </summary>
    /// <returns>200 OK with approved active access records.</returns>
    [Authorize]
    [HttpGet]
    public async Task<ActionResult<List<EmployerActiveAccessDto>>> GetActiveAccess(
        CancellationToken cancellationToken)
    {
        var employerId = GetCurrentEmployerId();

        if (employerId == null)
        {
            return Unauthorized();
        }

        try
        {
            var result = await _employerActiveAccessService.GetActiveAccessAsync(
                employerId.Value,
                cancellationToken);

            return Ok(result);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    /// <summary>
    /// View the value of a single approved permission.
    /// Text values are returned inline; file values are returned as a short-lived
    /// Supabase signed URL the client can use to render the file directly.
    /// </summary>
    [Authorize]
    [HttpGet("{permissionId}/view")]
    public async Task<ActionResult<EmployerAccessViewResultDto>> View(
        Guid permissionId,
        CancellationToken cancellationToken)
    {
        var employerId = GetCurrentEmployerId();
        if (employerId == null) return Unauthorized();

        try
        {
            var result = await _employerActiveAccessService.ViewAsync(
                employerId.Value, permissionId, cancellationToken);
            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { error = "PERMISSION_NOT_FOUND" });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("Supabase storage configuration"))
        {
            return StatusCode(503, new { error = "STORAGE_UNAVAILABLE", message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return UnprocessableEntity(new { error = "INVALID_STATE", message = ex.Message });
        }
    }

    private Guid? GetCurrentEmployerId()
    {
        var employerIdClaim = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                              ?? User.FindFirstValue("sub")
                              ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(employerIdClaim, out var employerId))
        {
            return null;
        }

        return employerId;
    }
}
