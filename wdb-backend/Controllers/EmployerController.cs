using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using wdb_backend.Abstractions;
using wdb_backend.DTOs;
using wdb_backend.Models;
using System.Security.Claims;
using wdb_backend.Usecases;

namespace wdb_backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmployerController : ControllerBase
{
    private readonly ICreateDataAccessRequestUsecase _createDataAccessUsecase;
    private readonly IFindWorkerInfosByEmailUsecase _findWorkerInfosUsecase;
    private readonly IWorkerService _workerService;
    private readonly IAddFlexibleWorkerInfoUsecase _addFlexibleWorkerInfoUsecase;

    public EmployerController(
        ICreateDataAccessRequestUsecase createDataAccessUsecase,
        IFindWorkerInfosByEmailUsecase findWorkerInfosUsecase,
        IWorkerService workerService,
        IAddFlexibleWorkerInfoUsecase addFlexibleWorkerInfoUsecase)
    {
        _createDataAccessUsecase = createDataAccessUsecase;
        _findWorkerInfosUsecase = findWorkerInfosUsecase;
        _workerService = workerService;
        _addFlexibleWorkerInfoUsecase = addFlexibleWorkerInfoUsecase;
    }

    /// <summary>Get a worker by email address.</summary>
    [HttpGet("GetWorkerByEmail")]
    public async Task<ActionResult<Worker>> GetWorkerByEmail(string email)
    {
        try
        {
            var worker = await _workerService.GetByEmailAsync(email);
            return Ok(worker);
        }
        catch (KeyNotFoundException)
        {
            return NotFound($"Worker {email} not found");
        }
    }

    /// <summary>Get all worker info fields visible to the current employer for a given worker email.</summary>
    [HttpGet]
    public async Task<ActionResult<List<WorkerInfoDto>>> GetWorkerInfosByEmail(string email)
    {
        var employerIdClaim = User.FindFirst("sub")?.Value
                              ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (employerIdClaim == null) return Unauthorized();

        var employerId = Guid.Parse(employerIdClaim);

        try
        {
            var workerInfos = await _findWorkerInfosUsecase.FindWorkerInfosByEmail(email, employerId);
            if (workerInfos.Count == 0) return Ok(new List<WorkerInfoDto>());

            var result = workerInfos.Select(w => new WorkerInfoDto
            {
                Id = w.Id,
                // Label: custom field label or preset field label
                Label = w.CustomLabel ?? w.Field?.Label ?? "Unknown",
                Category = w.Field?.Category?.CategoryName ?? "OtherInformation",
                Status = string.Empty
            }).ToList();

            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound($"Worker {email} not found");
        }
    }

    /// <summary>Get worker info fields that have already been requested by the current employer.</summary>
    [HttpGet("GetRequestedWorkerInfosByEmail")]
    public async Task<ActionResult<List<WorkerInfoDto>>> GetRequestedWorkerInfosByEmail(string email)
    {
        var employerIdClaim = User.FindFirst("sub")?.Value
                              ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (employerIdClaim == null) return Unauthorized();

        var employerId = Guid.Parse(employerIdClaim);

        try
        {
            var workerInfos = await _findWorkerInfosUsecase.FindRequestedWorkerInfosByEmail(email, employerId);
            if (workerInfos.Count == 0) return Ok(new List<WorkerInfoDto>());

            var result = workerInfos.Select(w => new WorkerInfoDto
            {
                Id = w.Id,
                Label = w.CustomLabel ?? w.Field?.Label ?? "Unknown",
                Category = w.Field?.Category?.CategoryName ?? "OtherInformation",
                Status = w.Permissions.FirstOrDefault()?.Status.ToString() ?? "Unknown"
            }).ToList();

            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound($"Worker {email} not found");
        }
    }

    /// <summary>Create a data access request for selected worker info fields.</summary>
    [Authorize]
    [HttpPost("AccessRequests")]
    public async Task<ActionResult> CreateRequest([FromBody] CreateRequestUsecaseDTO request)
    {
        var employerIdClaim = User.FindFirst("sub")?.Value
                              ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (employerIdClaim == null) return Unauthorized();

        var employerId = Guid.Parse(employerIdClaim);

        var allWorkerInfos = await _findWorkerInfosUsecase.FindWorkerInfosByEmail(request.Email, employerId);
        if (allWorkerInfos == null || allWorkerInfos.Count == 0) return NotFound();

        var selectedInfos = allWorkerInfos
            .Where(w => request.InfoDesc.Contains(w.Id.ToString()))
            .ToList();

        var workerId = allWorkerInfos[0].WorkerId;
        await _createDataAccessUsecase.CreateDataAccessRequest(selectedInfos, employerId, workerId, request.Reason);
        return Ok();
    }

    /// <summary>Request a worker to add a new custom field.</summary>
    [Authorize]
    [HttpPost("AddFlexibleWorkerInfo")]
    public async Task<ActionResult> AddFlexibleWorkerInfo([FromBody] AddFlexibleRequestDto request)
    {
        var employerIdClaim = User.FindFirst("sub")?.Value
                              ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (employerIdClaim == null) return Unauthorized();

        var employerId = Guid.Parse(employerIdClaim);

        try
        {
            await _addFlexibleWorkerInfoUsecase.ExecuteAsync(
                request.WorkerEmail, request.Category, request.Desc, request.Reason, employerId);
            return Ok();
        }
        catch (KeyNotFoundException)
        {
            return NotFound($"Worker {request.WorkerEmail} not found");
        }
    }
}
