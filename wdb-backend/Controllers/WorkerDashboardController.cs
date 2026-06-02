using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using wdb_backend.Abstractions;
using wdb_backend.DTOs;

namespace wdb_backend.Controllers;

[ApiController]
[Route("api/worker/dashboard")]
public class WorkerDashboardController : ControllerBase
{
    private readonly IWorkerDashboardService _workerDashboardService;

    public WorkerDashboardController(IWorkerDashboardService workerDashboardService)
    {
        _workerDashboardService = workerDashboardService;
    }

    /// <summary>
    /// Gets dashboard data for the currently logged-in worker.
    /// This endpoint is safer than accepting workerId from the frontend.
    /// </summary>
    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<WorkerDashboardResponseDto>> GetMyDashboard(
        CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub");

        if (string.IsNullOrWhiteSpace(userIdClaim))
        {
            return Unauthorized(new { message = "User ID claim is missing." });
        }

        if (!Guid.TryParse(userIdClaim, out var workerId))
        {
            return Unauthorized(new { message = "User ID claim is invalid." });
        }

        var dashboard = await _workerDashboardService.GetDashboardAsync(
            workerId,
            cancellationToken
        );

        if (dashboard == null)
        {
            return NotFound(new { message = "Worker dashboard not found." });
        }

        return Ok(dashboard);
    }

    /// <summary>
    /// Legacy endpoint kept for compatibility.
    /// Prefer GET /api/worker/dashboard/me for new frontend code.
    /// </summary>
    [HttpGet("{workerId:guid}")]
    public async Task<ActionResult<WorkerDashboardResponseDto>> GetDashboard(
        Guid workerId,
        CancellationToken cancellationToken)
    {
        var dashboard = await _workerDashboardService.GetDashboardAsync(
            workerId,
            cancellationToken
        );

        if (dashboard == null)
        {
            return NotFound();
        }

        return Ok(dashboard);
    }
}
