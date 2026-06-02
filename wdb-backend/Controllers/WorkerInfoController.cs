using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using wdb_backend.Abstractions;
using wdb_backend.DTOs;
using wdb_backend.Models;

namespace wdb_backend.Controllers;

/// <summary>
/// Worker profile API.
/// This controller manages worker-owned profile data.
/// Route: api/worker/profile
/// </summary>
[Authorize]
[ApiController]
[Route("api/worker/profile")]
public class WorkerInfoController : ControllerBase
{
    private readonly IWorkerInfoService _workerInfoService;

    public WorkerInfoController(IWorkerInfoService workerInfoService)
    {
        _workerInfoService = workerInfoService;
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    private Guid GetCurrentWorkerId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)
                 ?? User.FindFirst("sub");

        if (claim == null)
            throw new UnauthorizedAccessException("User ID not found in token");

        return Guid.Parse(claim.Value);
    }

    /// <summary>
    /// Map a WorkerInfo row or preset placeholder to the profile field DTO.
    /// Guid.Empty means this preset field has not been saved into worker_info yet.
    /// </summary>
    private static WorkerProfileFieldDto ToFieldDto(WorkerInfo w) => new()
    {
        InfoId = w.Id == Guid.Empty ? null : w.Id,
        FieldId = w.FieldId,
        Label = w.CustomLabel ?? w.Field?.Label ?? "Unknown",
        Type = w.Type,
        Value = w.Value,
        IsPreset = w.FieldId.HasValue
    };

    // ── GET /api/worker/profile ───────────────────────────────────────────

    /// <summary>
    /// Return all worker profile fields grouped by category.
    /// Preset fields are returned even if the worker has not filled them in yet.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetProfile(CancellationToken cancellationToken)
    {
        try
        {
            var workerId = GetCurrentWorkerId();

            var allFields = await _workerInfoService
                .GetAllWithPresetsAsync(workerId, cancellationToken);

            var grouped = allFields
                .GroupBy(w => w.Field?.Category?.CategoryName ?? "OtherInformation")
                .Select(g => new WorkerProfileCategoryDto
                {
                    Category = g.Key,
                    Fields = g.Select(ToFieldDto).ToList()
                })
                .ToList();

            return Ok(grouped);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    // ── PUT /api/worker/profile/preset ────────────────────────────────────

    /// <summary>
    /// Fill in or update the value of a preset field.
    /// The label and type are locked by the fields table.
    /// </summary>
    [HttpPut("preset")]
    public async Task<IActionResult> UpdatePreset(
        [FromBody] UpdatePresetFieldRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var workerId = GetCurrentWorkerId();

            if (request.FieldId == Guid.Empty)
                return BadRequest(new { message = "FieldId is required." });

            var workerInfo = new WorkerInfo
            {
                FieldId = request.FieldId,
                Value = request.Value,
                Type = "text" // Temporary value only. Repository replaces this using fields.allowed_type.
            };

            var updated = await _workerInfoService
                .UpdateAsync(workerId, workerInfo, cancellationToken);

            return Ok(ToFieldDto(updated));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex) when (ex.Message == "FIELD_NOT_FOUND")
        {
            return NotFound(new { message = "Preset field not found." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    // ── POST /api/worker/profile/custom ───────────────────────────────────

    /// <summary>
    /// Create a new custom OtherInformation field.
    /// Type is set at creation and cannot be changed later.
    /// </summary>
    [HttpPost("custom")]
    public async Task<IActionResult> CreateCustom(
        [FromBody] CreateCustomFieldRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var workerId = GetCurrentWorkerId();

            var type = request.Type.Trim().ToLowerInvariant();

            if (type != "text" && type != "file")
                return BadRequest(new { message = "Type must be 'text' or 'file'." });

            var workerInfo = new WorkerInfo
            {
                CustomLabel = request.Label,
                Type = type,
                Value = request.Value
            };

            var created = await _workerInfoService
                .CreateAsync(workerId, workerInfo, cancellationToken);

            return Ok(ToFieldDto(created));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (ArgumentException ex) when (ex.Message == "CUSTOM_LABEL_REQUIRED")
        {
            return BadRequest(new { message = "Custom field label is required." });
        }
        catch (ArgumentException ex) when (ex.Message == "INVALID_WORKER_INFO_TYPE")
        {
            return BadRequest(new { message = "Type must be 'text' or 'file'." });
        }
        catch (InvalidOperationException ex) when (ex.Message == "CUSTOM_LABEL_EXISTS")
        {
            return Conflict(new { message = "A custom field with this label already exists." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    // ── PUT /api/worker/profile/custom/{id} ───────────────────────────────

    /// <summary>
    /// Update label and/or value of a custom field.
    /// Type cannot be changed after creation.
    /// </summary>
    [HttpPut("custom/{id}")]
    public async Task<IActionResult> UpdateCustom(
        Guid id,
        [FromBody] UpdateCustomFieldRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var workerId = GetCurrentWorkerId();

            if (id == Guid.Empty)
                return BadRequest(new { message = "Field id is required." });

            var workerInfo = new WorkerInfo
            {
                Id = id,
                CustomLabel = request.Label,
                Value = request.Value,
                Type = "text" // Not used for custom update. Type is immutable.
            };

            var updated = await _workerInfoService
                .UpdateAsync(workerId, workerInfo, cancellationToken);

            return Ok(ToFieldDto(updated));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "Custom field not found." });
        }
        catch (ArgumentException ex) when (ex.Message == "CUSTOM_LABEL_REQUIRED")
        {
            return BadRequest(new { message = "Custom field label is required." });
        }
        catch (InvalidOperationException ex) when (ex.Message == "CUSTOM_LABEL_EXISTS")
        {
            return Conflict(new { message = "A custom field with this label already exists." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    // ── DELETE /api/worker/profile/custom/{id} ────────────────────────────

    /// <summary>
    /// Delete a custom field.
    /// Preset fields cannot be deleted.
    /// If the custom field has no permission history, it is physically deleted.
    /// If the custom field has active access, the worker must revoke access first.
    /// If the custom field has non-active access history, it is kept to preserve audit history.
    /// </summary>
    [HttpDelete("custom/{id}")]
    public async Task<IActionResult> DeleteCustom(
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            var workerId = GetCurrentWorkerId();

            if (id == Guid.Empty)
                return BadRequest(new { message = "Field id is required." });

            await _workerInfoService.DeleteAsync(workerId, id, cancellationToken);
            return NoContent();
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "Field not found." });
        }
        catch (InvalidOperationException ex) when (ex.Message == "PRESET_FIELD_CANNOT_BE_DELETED")
        {
            return BadRequest(new { message = "Preset fields cannot be deleted. You can clear the value instead." });
        }
        catch (InvalidOperationException ex) when (ex.Message == "ACTIVE_PERMISSION_EXISTS")
        {
            return Conflict(new { message = "This field has active access. Please revoke access first." });
        }
        catch (InvalidOperationException ex) when (ex.Message == "PERMISSION_HISTORY_EXISTS")
        {
            return Conflict(new { message = "This field has access history and cannot be deleted. You can clear the value instead." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    // ── Legacy endpoint ───────────────────────────────────────────────────

    /// <summary>
    /// Legacy: get all worker info as a flat list.
    /// Prefer GET /api/worker/profile for the new profile page.
    /// </summary>
    [HttpGet("all")]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        try
        {
            var workerId = GetCurrentWorkerId();
            var infos = await _workerInfoService.GetAllAsyncHash(workerId, cancellationToken);
            return Ok(infos);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }
}
