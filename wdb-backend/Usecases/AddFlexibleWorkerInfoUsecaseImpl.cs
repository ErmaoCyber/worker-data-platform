using wdb_backend.Abstractions;
using wdb_backend.DTOs;
using wdb_backend.Models;
namespace wdb_backend.Usecases;


public class AddFlexibleWorkerInfoUsecaseImpl : IAddFlexibleWorkerInfoUsecase
{
    private readonly IWorkerService _workerService;
    private readonly IWorkerInfoService _workerInfoService;
    private readonly IRequestService _requestService;



    // The constructor injects the necessary services for the use case to function.
    public AddFlexibleWorkerInfoUsecaseImpl(IWorkerService workerService, IWorkerInfoService workerInfoService, IRequestService requestService)
    {
        _workerService = workerService;
        _workerInfoService = workerInfoService;
        _requestService = requestService;
    }


    // This use case allows an employer to request a worker to add flexible information that is not predefined in the system.
    // For example, an employer might want to request a worker to add their LinkedIn profile or a personal statement. 
    // The employer can specify the category and description of the information they want the worker to add. The system will create a new worker info entry with an empty value and send a request to the worker to fill in the information.
    public async Task ExecuteAsync(string workerEmail, string category, string desc, string reason, Guid employerId, CancellationToken cancellationToken = default)
    {
        // TODO: Re-implement using new schema (custom_label workflow in worker_info)
        // pending the custom_request feature design.
        /*
        var worker = await _workerService.GetByEmailAsync(workerEmail);
        var workerInfo = new WorkerInfo
        {
            WorkerId = worker.Id,
            Category = Enum.Parse<Enums.WorkerInfoCategory>(category),
            Desc = desc,
            Value = ""
        };
        await _workerInfoService.CreateAsync(worker.Id, workerInfo);
        await _requestService.CreateAsync(employerId, worker.Id, reason, cancellationToken);
        */
        await Task.CompletedTask;
    }
}