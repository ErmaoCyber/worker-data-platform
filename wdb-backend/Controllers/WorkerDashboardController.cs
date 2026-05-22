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
