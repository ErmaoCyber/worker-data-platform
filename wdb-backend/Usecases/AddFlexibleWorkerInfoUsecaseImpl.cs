using wdb_backend.Abstractions;
using wdb_backend.Models;

namespace wdb_backend.Usecases;

/// <summary>
/// Allows an employer to request that a worker adds a new custom field.
/// Creates a WorkerInfo placeholder and a linked Request.
/// </summary>
public class AddFlexibleWorkerInfoUsecaseImpl : IAddFlexibleWorkerInfoUsecase
{
    private readonly IWorkerService _workerService;
    private readonly IWorkerInfoService _workerInfoService;
    private readonly IRequestService _requestService;

    public AddFlexibleWorkerInfoUsecaseImpl(
        IWorkerService workerService,
        IWorkerInfoService workerInfoService,
        IRequestService requestService)
    {
        _workerService = workerService;
        _workerInfoService = workerInfoService;
        _requestService = requestService;
    }

    /// <summary>
    /// Create a custom WorkerInfo placeholder (no value yet) and a Request so the
    /// worker knows they are expected to fill it in.
    /// </summary>
    public async Task ExecuteAsync(
        string workerEmail,
        string category,
        string desc,
        string reason,
        Guid employerId,
        CancellationToken cancellationToken = default)
    {
        var worker = await _workerService.GetByEmailAsync(workerEmail);

        // Create a custom (Other) field — field_id is null, custom_label is set
        var workerInfo = new WorkerInfo
        {
            WorkerId = worker.Id,
            CustomLabel = desc,      // replaces old Desc + Category
            Type = "text",    // default; employer/worker can negotiate in Phase 2
            Value = null       // not filled in yet
        };

        await _workerInfoService.CreateAsync(worker.Id, workerInfo);
        await _requestService.CreateAsync(employerId, worker.Id, reason, cancellationToken);
    }
}
