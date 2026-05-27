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
