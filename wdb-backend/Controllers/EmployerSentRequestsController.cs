using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using wdb_backend.Abstractions;
using wdb_backend.DTOs;

namespace wdb_backend.Controllers;

[ApiController]
[Route("api/Employer/sent-requests")]
public class EmployerSentRequestsController : ControllerBase
{
    private readonly IEmployerSentRequestService _employerSentRequestService;

    public EmployerSentRequestsController(
        IEmployerSentRequestService employerSentRequestService)
    {
        _employerSentRequestService = employerSentRequestService;
    }

    /// <summary>
    /// Get all data access requests sent by the current employer.
    /// </summary>
    /// <returns>200 OK with the employer's sent requests.</returns>
    [Authorize]
    [HttpGet]
    public async Task<ActionResult<List<EmployerSentRequestDto>>> GetSentRequests(
        CancellationToken cancellationToken)
    {
        var employerId = GetCurrentEmployerId();

        if (employerId == null)
        {
            return Unauthorized();
        }

        try
        {
            var result = await _employerSentRequestService.GetSentRequestsAsync(
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
