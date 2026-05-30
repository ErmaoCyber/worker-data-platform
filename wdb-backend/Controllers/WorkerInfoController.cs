using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using wdb_backend.Abstractions;
using wdb_backend.DTOs;
using wdb_backend.Models;
using System.Security.Claims;

namespace wdb_backend.Controllers;

/// <summary>
/// Worker profile API — manages personal data fields.
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

    /// <summary>Map a WorkerInfo row to the profile field DTO.</summary>
    private static WorkerProfileFieldDto ToFieldDto(WorkerInfo w) => new()
    {
        InfoId = w.Id,
        FieldId = w.FieldId,
        Label = w.CustomLabel ?? w.Field?.Label ?? "Unknown",
        Type = w.Type,
        Value = w.Value,
        IsPreset = w.FieldId.HasValue
    };

    // ── GET /api/worker/profile ───────────────────────────────────────────

    /// <summary>
    /// Return all fields grouped by category.
    /// Preset fields with no value are included (Value = null).
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetProfile(CancellationToken cancellationToken)
    {
        try
        {
            var workerId = GetCurrentWorkerId();
            var allFields = await _workerInfoService.GetAllWithPresetsAsync(workerId, cancellationToken);

            // Group by category name
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
    /// Label and type are locked — only value can change.
    /// </summary>
    [HttpPut("preset")]
    public async Task<IActionResult> UpdatePreset(
        [FromBody] UpdatePresetFieldRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var workerId = GetCurrentWorkerId();

            var workerInfo = new WorkerInfo
            {
                FieldId = request.FieldId,
                Value = request.Value,
                Type = string.Empty   // not used for update; type is fixed on creation
            };

            var updated = await _workerInfoService.UpdateAsync(workerId, workerInfo, cancellationToken);
            return Ok(ToFieldDto(updated));
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

    // ── POST /api/worker/profile/custom ───────────────────────────────────

    /// <summary>
    /// Create a new custom (Other) field.
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

            if (request.Type != "text" && request.Type != "file")
                return BadRequest(new { message = "Type must be 'text' or 'file'" });

            var workerInfo = new WorkerInfo
            {
                CustomLabel = request.Label,
                Type = request.Type,
                Value = request.Value
            };

            var created = await _workerInfoService.CreateAsync(workerId, workerInfo, cancellationToken);
            return Ok(ToFieldDto(created));
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

            var workerInfo = new WorkerInfo
            {
                Id = id,
                CustomLabel = request.Label,
                Value = request.Value,
                Type = string.Empty   // type is immutable, not used here
            };

            var updated = await _workerInfoService.UpdateAsync(workerId, workerInfo, cancellationToken);
            return Ok(ToFieldDto(updated));
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

    // ── DELETE /api/worker/profile/custom/{id} ────────────────────────────

    /// <summary>
    /// Delete a custom field.
    /// Returns 409 if the field has an active (approved) permission —
    /// worker must revoke access first.
    /// </summary>
    [HttpDelete("custom/{id}")]
    public async Task<IActionResult> DeleteCustom(
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            var workerId = GetCurrentWorkerId();
            await _workerInfoService.DeleteAsync(workerId, id, cancellationToken);
            return NoContent();
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "Field not found" });
        }
        catch (InvalidOperationException ex) when (ex.Message == "ACTIVE_PERMISSION_EXISTS")
        {
            return Conflict(new { message = "This field has active access. Please revoke access first." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    // ── Legacy endpoints (kept for backward compatibility) ────────────────

    /// <summary>Legacy: get all worker info as a flat list. Use GET /api/worker/profile instead.</summary>
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
