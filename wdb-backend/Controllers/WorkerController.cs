using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using wdb_backend.Abstractions;
using wdb_backend.DTOs;
using wdb_backend.Models;
using System.Security.Claims;

namespace wdb_backend.Controllers;

/// <summary>API controller for worker-related operations.</summary>
[ApiController]
[Route("api/[controller]")]
public class WorkerController : ControllerBase
{
    private readonly IPermissionService _permissionService;
    private readonly IRequestService _requestService;
    private readonly IWorkerInfoService _workerInfoService;
    private readonly IEmployerService _employerService;
    private readonly IActiveAccessService _activeAccessService;

    public WorkerController(
        IPermissionService permissionService,
        IRequestService requestService,
        IWorkerInfoService workerInfoService,
        IEmployerService employerService,
        IActiveAccessService activeAccessService)
    {
        _permissionService = permissionService;
        _requestService = requestService;
        _workerInfoService = workerInfoService;
        _employerService = employerService;
        _activeAccessService = activeAccessService;
    }

    // ── Nested response types ─────────────────────────────────────────────

    public class FieldResponse
    {
        public required string Id { get; set; }
        public required string Label { get; set; }
        public required bool Checked { get; set; } = false;
    }

    public class RequestRowResponse
    {
        public required string Id { get; set; }
        public required string Company { get; set; }
        public required string Date { get; set; }
        public required List<FieldResponse> ListedInfo { get; set; }
        public required List<FieldResponse> UnlistedInfo { get; set; }
        public required string Reason { get; set; }
    }

    // ── Endpoints ─────────────────────────────────────────────────────────

    /// <summary>Get pending permissions for a worker.</summary>
    [HttpGet("{workerId}/permissions")]
    public async Task<ActionResult<List<Permission>>> GetPermissions(Guid workerId)
    {
        var result = await _permissionService.GetAllByWorkerIdAsync(workerId, 0);
        if (result == null) return NotFound(new { error = "WORKER_NOT_FOUND" });
        return Ok(result);
    }

    /// <summary>Get all requests for a worker, with grouped field info.</summary>
    [HttpGet("{workerId}/requests")]
    public async Task<ActionResult> GetRequests(Guid workerId)
    {
        var requests = await _requestService.GetAllByWorkerIdAsync(workerId);
        var permissions = await _permissionService.GetAllByWorkerIdAsync(workerId);
        var workerInfos = await _workerInfoService.GetAllAsync(workerId);

        var rows = new List<RequestRowResponse>();

        foreach (var request in requests)
        {
            var employer = await _employerService.GetEmployerInfoAsync(request.EmployerId);
            var permsForRequest = permissions.Where(p => p.RequestId == request.Id).ToList();

            var listedInfos = new List<FieldResponse>();
            var unlistedInfos = new List<FieldResponse>();

            foreach (var p in permsForRequest)
            {
                var info = workerInfos.FirstOrDefault(w => w.Id == p.InfoId);
                var label = info?.CustomLabel ?? info?.Field?.Label ?? "Unknown";

                if (info?.Value != null)
                    listedInfos.Add(new FieldResponse { Id = p.Id.ToString(), Label = label, Checked = false });
                else
                    unlistedInfos.Add(new FieldResponse { Id = p.Id.ToString(), Label = label, Checked = false });
            }

            rows.Add(new RequestRowResponse
            {
                Id = request.Id.ToString(),
                Company = employer?.Name ?? "Unknown",
                Date = request.CreatedAt.ToString("dd.MM.yyyy hh:mm tt"),
                ListedInfo = listedInfos,
                UnlistedInfo = unlistedInfos,
                Reason = request.Reason
            });
        }

        return Ok(rows);
    }

    /// <summary>Get active (approved + not expired) access entries for a worker.</summary>
    [HttpGet("{workerId}/active-access")]
    public async Task<ActionResult<List<ActiveAccessDto>>> GetActiveAccess(
        Guid workerId,
        [FromQuery] string? company = null,
        [FromQuery] string? dataType = null)
    {
        var result = await _activeAccessService.GetActiveAccessAsync(workerId, company, dataType);
        return Ok(result);
    }
}
