using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using wdb_backend.Abstractions;
using wdb_backend.DTOs;
using wdb_backend.Models;
using wdb_backend.Services;
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

    public EmployerController(ICreateDataAccessRequestUsecase createDataAccessUsecase,
        IFindWorkerInfosByEmailUsecase findWorkerInfosUsecase, IWorkerService workerService, IAddFlexibleWorkerInfoUsecase addFlexibleWorkerInfoUsecase)
    {
        _createDataAccessUsecase = createDataAccessUsecase;
        _findWorkerInfosUsecase = findWorkerInfosUsecase;
        _workerService = workerService;
        _addFlexibleWorkerInfoUsecase = addFlexibleWorkerInfoUsecase;
    }

    /// <summary>
    /// Get the worker by Email.
    /// </summary>
    /// <param name="email">The worker's email address.</param>
    /// <returns>200 OK with list of worker info, or 4404 Notfound if worker not found</returns>
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
    /// Get all of the worker's information by Email.
    /// </summary>
    /// <param name="email">The worker's email address.</param>
    /// <returns>200 OK with list of worker info, or 4404 Notfound if worker not found</returns>
    [HttpGet]
    public async Task<ActionResult<List<WorkerInfoDto>>> GetWorkerInfosByEmail(string email)
    {
        var employerIdClaim = User.FindFirst("sub")?.Value
                              ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (employerIdClaim == null)
            return Unauthorized();

        var employerId = Guid.Parse(employerIdClaim);
        try
        {
            var workerInfos = await _findWorkerInfosUsecase.FindWorkerInfosByEmail(email, employerId);
            if (workerInfos.Count == 0)
            {
                return Ok(new List<WorkerInfoDto>());
            }

            var result = workerInfos.Select(w => new WorkerInfoDto()
            {
                Id = w.Id,
                Desc = w.Desc,
            }).ToList();
            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound($"Worker {email} not found");
        }
    }


    /// <summary>
    /// Get all of the worker's information by Email.
    /// </summary>
    /// <param name="email">The worker's email address.</param>
    /// <returns>200 OK with list of worker info, or 4404 Notfound if worker not found</returns>
    [HttpGet("GetRequestedWorkerInfosByEmail")]
    public async Task<ActionResult<List<WorkerInfoDto>>> GetRequestedWorkerInfosByEmail(string email)
    {
        var employerIdClaim = User.FindFirst("sub")?.Value
                              ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (employerIdClaim == null)
            return Unauthorized();

        var employerId = Guid.Parse(employerIdClaim);
        try
        {
            var workerInfos = await _findWorkerInfosUsecase.FindRequestedWorkerInfosByEmail(email, employerId);
            if (workerInfos.Count == 0)
            {
                return Ok(new List<WorkerInfoDto>());
            }

            var result = workerInfos.Select(w => new WorkerInfoDto()
            {
                Id = w.Id,
                Desc = w.Desc,
                Status = w.Permissions.FirstOrDefault()?.Status.ToString() ?? "Unknown"
            }).ToList();
            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound($"Worker {email} not found");
        }
    }


    /// <summary>
    /// Creates a data access request for selected worker information.
    /// </summary>
    /// <returns>200 OK if successful, 404 if worker not found.</returns>
    [Authorize]
    [HttpPost("AccessRequests")]
    public async Task<ActionResult> CreateRequest([FromBody] CreateRequestUsecaseDTO request)
    {
        // get employer id from the user's token
        // var employerIdClaim = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        // Console.WriteLine("Print:"+employerIdClaim);
        var employerIdClaim = User.FindFirst("sub")?.Value
                              ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        Console.WriteLine("Print:" + employerIdClaim);
        if (employerIdClaim == null)
        {
            return Unauthorized();
        }

        var employerId = Guid.Parse(employerIdClaim);

        var allWorkerInfos = await _findWorkerInfosUsecase.FindWorkerInfosByEmail(request.Email, employerId);
        if (allWorkerInfos == null || allWorkerInfos.Count == 0)
        {
            return NotFound();
        }

        var selectedInfos = allWorkerInfos.Where(w => request.InfoDesc.Contains(w.Id.ToString()))
            .ToList();
        var worker_id = allWorkerInfos[0].WorkerId;
        await _createDataAccessUsecase.CreateDataAccessRequest(selectedInfos, employerId, worker_id, request.Reason);
        return Ok();
    }



    /// <summary>    
    /// Adds flexible worker information for a specific worker.
    /// </summary>
    /// <param name="request">The request containing worker email, category, description, and employer ID.</param>
    /// <returns>200 OK if successful, 404 if worker not found.</returns>
    [Authorize]
    [HttpPost("AddFlexibleWorkerInfo")]
    public async Task<ActionResult> AddFlexibleWorkerInfo([FromBody] AddFlexibleRequestDto request)
    {            // get employer id from the user's token
        var employerIdClaim = User.FindFirst("sub")?.Value
                              ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (employerIdClaim == null)
        {
            return Unauthorized();
        }

        var employerId = Guid.Parse(employerIdClaim);

        try
        {
            await _addFlexibleWorkerInfoUsecase.ExecuteAsync(request.WorkerEmail, request.Category, request.Desc, employerId);
            return Ok();
        }
        catch (KeyNotFoundException)
        {
            return NotFound($"Worker {request.WorkerEmail} not found");
        }
    }

}
