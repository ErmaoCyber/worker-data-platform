using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using wdb_backend.Abstractions;
using wdb_backend.DTOs;
using wdb_backend.Models;
using System.Security.Claims;

namespace wdb_backend.Controllers;

/// <summary>
/// API controller for managing worker-related operations.
/// </summary>
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

    [HttpGet("{workerId}/permissions")]
    public async Task<ActionResult<List<Permission>>> GetPermissions(Guid workerId)
    {
        var result = await _permissionService.GetAllByWorkerIdAsync(workerId, 0);

        if (result == null)
        {
            return NotFound(new { error = "WORKER_NOT_FOUND" });
        }

        return Ok(result);
    }

    [HttpGet("{workerId}/requests")]
    public async Task<ActionResult<Request>> GetRequestReason(Guid requestId)
    {
        var result = await _requestService.GetByRequestIdAsync(requestId);

        if (result == null)
        {
            return NotFound(new { error = "REQUEST_NOT_FOUND" });
        }

        return Ok(result);
    }

    [HttpGet("{workerId}/info")]
    public async Task<ActionResult<Request>> GetAllWorkerInfo(Guid workerId)
    {
        var result = await _workerInfoService.GetAllAsync(workerId);

        if (result == null)
        {
            return NotFound(new { error = "WORKER_NOT_FOUND" });
        }

        return Ok(result);
    }

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

    [Authorize]
    [HttpGet("rows")]
    public async Task<ActionResult> GetRows()
    {
        var workerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (workerId == null)
        {
            return Unauthorized();
        }

        var workerGuid = Guid.Parse(workerId);

        var requests = await _requestService.GetAllByWorkerIdAsync(workerGuid);
        var workerInfo = await _workerInfoService.GetAllAsync(workerGuid);
        var permissions = await _permissionService.GetAllByWorkerIdAsync(workerGuid, 0);
        var groupedPermissions = permissions.GroupBy(permission => permission.RequestId);

        var employers = await _employerService.GetDistinctEmployers();
        var employerMap = employers.ToDictionary(employer => employer.Id);

        var rows = new List<RequestRowResponse>();

        foreach (var group in groupedPermissions)
        {
            var request = requests.FirstOrDefault(requestItem => requestItem.Id == group.Key);

            if (request == null)
            {
                continue;
            }

            employerMap.TryGetValue(request.EmployerId, out var employer);

            var listedInfos = new List<FieldResponse>();
            var unlistedInfos = new List<FieldResponse>();
            foreach (var p in group)
            {
                var info = workerInfo.FirstOrDefault(w => w.Id == p.InfoId);
                if (info?.Value != null) {
                    listedInfos.Add(new FieldResponse
                    {
                        Id = p.Id.ToString(),
                        Label = info.Desc ?? "Unknown",
                        Checked = false
                    });
                } else if (info?.Value == null)
                {
                    unlistedInfos.Add(new FieldResponse
                    {
                        Id = p.Id.ToString(),
                        Label = info?.Desc ?? "Unknown",
                        Checked = false
                    });
                }

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

    [HttpGet("{workerId}/active-access")]
    public async Task<ActionResult<List<ActiveAccessDto>>> GetActiveAccess(
        Guid workerId,
        [FromQuery] string? company = null,
        [FromQuery] string? dataType = null)
    {
        var result = await _activeAccessService.GetActiveAccessAsync(
            workerId,
            company,
            dataType);

        return Ok(result);
    }

<<<<<<< HEAD
    [HttpGet("GetCategoryFields")]
    public ActionResult<Dictionary<string, List<string>>> GetCategoryFields()
    {
        var fields = _workerInfoService.GetCategoryFields();
        return Ok(fields);
    }
=======

    // [HttpGet("unlistedinfo")]
    // public async Task<ActionResult> GetUnlistedeInfo()
    // {

    //     var workerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    //     if (workerId == null) return Unauthorized();
    //     var workerGuid = Guid.Parse(workerId);

    //     var requests = await _requestService.GetAllByWorkerIdAsync(workerGuid);
    //     var workerInfo = await _workerInfoService.GetAllAsync(workerGuid);
    //     var permissions = await _permissionService.GetAllByWorkerIdAsync(workerGuid, 0);
    //     var groupedPermissions = permissions.GroupBy(p => p.RequestId);

    //     var employers = await _employerService.GetDistinctEmployers();
    //     var employerMap = employers.ToDictionary(e => e.Id);

    //     var rows = new List<RequestRowResponse>();

    //     foreach (var group in groupedPermissions)
    //     {
    //         var request = requests.FirstOrDefault(p => p.Id == group.Key);
    //         if (request == null) continue;

    //         employerMap.TryGetValue(request.EmployerId, out var employer);

    //         var workerInfos = new List<FieldResponse>();
    //         foreach (var p in group)
    //         {
    //             var info = workerInfo.FirstOrDefault(w => w.Id == p.InfoId);


    //             workerInfos.Add(new FieldResponse
    //             {
    //                 Id = p.Id.ToString(),
    //                 Label = info?.Desc ?? "Unknown",
    //                 Checked = false
    //             });
    //         }
    //         rows.Add(new RequestRowResponse
    //         {
    //             Id = request.Id.ToString(),
    //             Company = employer?.Name.ToString() ?? "Unknown",
    //             Date = request.CreatedAt.ToString("dd.MM.yyyy hh:mm tt"),
    //             Fields = workerInfos,
    //             Reason = request.Reason
    //         });

    //     }
    //     ;
    //     return Ok(rows);

    // }


>>>>>>> origin/main
}
