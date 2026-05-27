using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using wdb_backend.Abstractions;
using wdb_backend.Dtos;

namespace wdb_backend.Controllers;

/// <summary>
/// Provides audit log records for the logged-in worker.
/// </summary>
[ApiController]
[Route("api/worker/audit-log")]
[Authorize]
public class WorkerAuditLogController : ControllerBase
{
    private readonly IWorkerAuditLogService _auditLogService;

    public WorkerAuditLogController(IWorkerAuditLogService auditLogService)
    {
        _auditLogService = auditLogService;
    }

    /// <summary>
    /// Gets blockchain audit log records for the currently logged-in worker.
    /// </summary>
    /// <returns>
    /// A list of audit log records linked to the worker's blockchain address.
    /// </returns>
    [HttpGet("me")]
    public async Task<ActionResult<WorkerAuditLogResponseDto>> GetMyAuditLog()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub");

        if (string.IsNullOrWhiteSpace(userIdClaim))
        {
            return Unauthorized("User ID claim is missing.");
        }

        if (!Guid.TryParse(userIdClaim, out var workerId))
        {
            return Unauthorized("User ID claim is invalid.");
        }

        try
        {
            var result = await _auditLogService.GetWorkerAuditLogAsync(workerId);
            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound("Worker not found.");
        }
    }
}
