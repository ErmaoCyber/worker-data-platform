using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using wdb_backend.Abstractions;
using wdb_backend.DTOs;

namespace wdb_backend.Controllers;

/// <summary>
/// Worker-side data access review API.
/// Used by the worker Data Access page.
/// </summary>
[Authorize]
[ApiController]
[Route("api/worker/data-access")]
public class WorkerRequestReviewController : ControllerBase
{
    private readonly IWorkerRequestReviewService _reviewService;

    public WorkerRequestReviewController(IWorkerRequestReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    private Guid GetCurrentWorkerId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)
                 ?? User.FindFirst("sub");

        if (claim == null)
            throw new UnauthorizedAccessException("User ID not found in token");

        return Guid.Parse(claim.Value);
    }

    /// <summary>
    /// Return active pending requests for the current worker.
    /// </summary>
    [HttpGet("active-requests")]
    public async Task<IActionResult> GetActiveRequests(CancellationToken cancellationToken)
    {
        try
        {
            var workerId = GetCurrentWorkerId();

            var requests = await _reviewService.GetActiveRequestsAsync(
                workerId,
                cancellationToken);

            return Ok(requests);
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

    /// <summary>
    /// Submit approve/reject decisions for one request.
    /// </summary>
    [HttpPost("requests/{requestId}/review")]
    public async Task<IActionResult> SubmitReview(
        Guid requestId,
        [FromBody] SubmitWorkerRequestReviewRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var workerId = GetCurrentWorkerId();

            await _reviewService.SubmitReviewAsync(
                workerId,
                requestId,
                request,
                cancellationToken);

            return Ok(new { message = "Request review submitted." });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex) when (ex.Message == "REQUEST_NOT_FOUND")
        {
            return NotFound(new { message = "Request not found." });
        }
        catch (KeyNotFoundException ex) when (ex.Message == "PERMISSION_NOT_FOUND")
        {
            return NotFound(new { message = "Permission not found." });
        }
        catch (InvalidOperationException ex) when (ex.Message == "NO_REVIEW_ITEMS")
        {
            return BadRequest(new { message = "No review items were provided." });
        }
        catch (InvalidOperationException ex) when (ex.Message == "DUPLICATE_REVIEW_ITEM")
        {
            return BadRequest(new { message = "The same permission was reviewed more than once." });
        }
        catch (InvalidOperationException ex) when (ex.Message == "INVALID_DECISION")
        {
            return BadRequest(new { message = "Decision must be 'approved' or 'rejected'." });
        }
        catch (InvalidOperationException ex) when (ex.Message == "FIELD_VALUE_REQUIRED")
        {
            return BadRequest(new { message = "This field has no saved value and cannot be approved." });
        }
        catch (InvalidOperationException ex) when (ex.Message == "REQUEST_EXPIRED")
        {
            return Conflict(new { message = "This request has expired and cannot be reviewed." });
        }
        catch (InvalidOperationException ex) when (ex.Message == "PERMISSION_NOT_PENDING")
        {
            return Conflict(new { message = "Only pending permissions can be reviewed." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }
}
