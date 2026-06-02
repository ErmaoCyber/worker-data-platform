using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using wdb_backend.Abstractions;
using wdb_backend.DTOs;
using wdb_backend.Models;

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

    private Guid GetCurrentEmployerId()
    {
        var employerIdClaim = User.FindFirst("sub")?.Value
                              ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (employerIdClaim == null)
            throw new UnauthorizedAccessException("Employer ID not found in token.");

        return Guid.Parse(employerIdClaim);
    }

    /// <summary>
    /// Get a worker by email address.
    /// </summary>
    [Authorize]
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

    /// <summary>
    /// Get all worker fields that the current employer can request.
    /// Preset fields return Id = fieldId.
    /// Custom fields return Id = workerInfoId.
    /// </summary>
    [Authorize]
    [HttpGet]
    public async Task<ActionResult<List<WorkerInfoDto>>> GetWorkerInfosByEmail(
        string email,
        CancellationToken cancellationToken)
    {
        try
        {
            var employerId = GetCurrentEmployerId();

            var workerInfos = await _findWorkerInfosUsecase
                .FindWorkerInfosByEmail(email, employerId, cancellationToken);

            if (workerInfos.Count == 0)
                return Ok(new List<WorkerInfoDto>());

            var result = workerInfos.Select(ToEmployerWorkerInfoDto).ToList();

            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (KeyNotFoundException)
        {
            return NotFound($"Worker {email} not found");
        }
    }

    /// <summary>
    /// Get worker fields that have already been requested by the current employer.
    /// </summary>
    [Authorize]
    [HttpGet("GetRequestedWorkerInfosByEmail")]
    public async Task<ActionResult<List<WorkerInfoDto>>> GetRequestedWorkerInfosByEmail(
        string email,
        CancellationToken cancellationToken)
    {
        try
        {
            var employerId = GetCurrentEmployerId();

            var workerInfos = await _findWorkerInfosUsecase
                .FindRequestedWorkerInfosByEmail(email, employerId, cancellationToken);

            if (workerInfos.Count == 0)
                return Ok(new List<WorkerInfoDto>());

            var result = workerInfos.Select(ToEmployerWorkerInfoDto).ToList();

            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (KeyNotFoundException)
        {
            return NotFound($"Worker {email} not found");
        }
    }

    /// <summary>
    /// Create a data access request for selected worker fields.
    /// Also supports an optional customRequest for a new field the worker has not created yet.
    /// </summary>
    [Authorize]
    [HttpPost("AccessRequests")]
    public async Task<ActionResult> CreateRequest(
        [FromBody] CreateRequestUsecaseDTO request,
        CancellationToken cancellationToken)
    {
        try
        {
            var employerId = GetCurrentEmployerId();

            var worker = await _workerService.GetByEmailAsync(
                request.Email,
                cancellationToken);

            var selectedItemIds = request.InfoDesc
                .Select(id =>
                {
                    var ok = Guid.TryParse(id, out var parsed);
                    if (!ok)
                        throw new InvalidOperationException("INVALID_SELECTED_ITEM_ID");

                    return parsed;
                })
                .ToList();

            await _createDataAccessUsecase.CreateDataAccessRequest(
                selectedItemIds,
                employerId,
                worker.Id,
                request.Reason,
                request.CustomRequest,
                cancellationToken);

            return Ok(new { message = "Access request created." });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex) when (ex.Message == "SELECTED_ITEM_NOT_FOUND")
        {
            return NotFound(new { message = "Selected field was not found." });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = $"Worker {request.Email} not found." });
        }
        catch (InvalidOperationException ex) when (ex.Message == "INVALID_SELECTED_ITEM_ID")
        {
            return BadRequest(new { message = "Selected item id is not a valid GUID." });
        }
        catch (InvalidOperationException ex) when (ex.Message == "NO_SELECTED_ITEMS")
        {
            return BadRequest(new { message = "Please select at least one field or provide a custom request." });
        }
        catch (InvalidOperationException ex) when (ex.Message == "REASON_REQUIRED")
        {
            return BadRequest(new { message = "Reason is required." });
        }
        catch (InvalidOperationException ex) when (ex.Message == "ITEM_ALREADY_REQUESTED")
        {
            return Conflict(new { message = "One or more selected fields have already been requested." });
        }
    }

    /// <summary>
    /// Request a worker to add a new custom field.
    /// This remains as the older separate flexible request flow for now.
    /// </summary>
    [Authorize]
    [HttpPost("AddFlexibleWorkerInfo")]
    public async Task<ActionResult> AddFlexibleWorkerInfo(
        [FromBody] AddFlexibleRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var employerId = GetCurrentEmployerId();

            await _addFlexibleWorkerInfoUsecase.ExecuteAsync(
                request.WorkerEmail,
                request.Category,
                request.Desc,
                request.Reason,
                employerId,
                cancellationToken);

            return Ok(new { message = "Flexible worker info request created." });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (KeyNotFoundException)
        {
            return NotFound($"Worker {request.WorkerEmail} not found");
        }
    }

    private static WorkerInfoDto ToEmployerWorkerInfoDto(WorkerInfo workerInfo)
    {
        var isPreset = workerInfo.FieldId.HasValue;

        return new WorkerInfoDto
        {
            Id = isPreset
                ? workerInfo.FieldId!.Value
                : workerInfo.Id,

            InfoId = workerInfo.Id == Guid.Empty ? null : workerInfo.Id,
            FieldId = workerInfo.FieldId,
            Label = workerInfo.CustomLabel ?? workerInfo.Field?.Label ?? "Unknown",
            Category = workerInfo.Field?.Category?.CategoryName ?? "OtherInformation",
            Type = workerInfo.Type,
            Status = workerInfo.Permissions.FirstOrDefault()?.Status.ToString(),
            IsPreset = isPreset,
            HasValue = !string.IsNullOrWhiteSpace(workerInfo.Value)
        };
    }
}
