using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using wdb_backend.Abstractions;
using wdb_backend.DTOs;

namespace wdb_backend.Controllers;

/// <summary>
/// API controller for the employer's create-request flow.
/// Exposes the field catalog used by the form and accepts new request submissions.
/// </summary>
[ApiController]
[Authorize]
[Route("api/employer-request")]
public class EmployerRequestController : ControllerBase
{
    private readonly IEmployerRequestService _employerRequestService;

    public EmployerRequestController(IEmployerRequestService employerRequestService)
    {
        _employerRequestService = employerRequestService;
    }

    /// <summary>
    /// Catalog of fields the employer can ask the target worker for.
    /// Preset fields are grouped by category; the OtherInformation category lists
    /// only the worker's own custom items.
    /// </summary>
    [HttpGet("catalog")]
    public async Task<ActionResult<EmployerRequestCatalogDto>> GetCatalog(
        [FromQuery] string email,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return BadRequest(new { error = "EMAIL_REQUIRED" });
        }

        var catalog = await _employerRequestService.GetCatalogAsync(email, cancellationToken);
        if (catalog == null)
        {
            return NotFound(new { error = "WORKER_NOT_FOUND" });
        }

        return Ok(catalog);
    }

    /// <summary>
    /// Create a new data access request. Accepts any mix of preset fields,
    /// existing custom items, and a free-text custom request.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<CreateEmployerRequestResultDto>> Create(
        [FromBody] CreateEmployerRequestDto dto,
        CancellationToken cancellationToken)
    {
        var employerIdClaim = User.FindFirst("sub")?.Value
                              ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (employerIdClaim == null)
        {
            return Unauthorized();
        }

        var employerId = Guid.Parse(employerIdClaim);

        try
        {
            var result = await _employerRequestService.CreateAsync(
                employerId, dto, cancellationToken);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = "WORKER_NOT_FOUND", message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = "INVALID_REQUEST", message = ex.Message });
        }
    }
}
