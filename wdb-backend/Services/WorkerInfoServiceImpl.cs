using wdb_backend.Abstractions;
using wdb_backend.Models;

namespace wdb_backend.Services;

public class WorkerInfoServiceImpl : IWorkerInfoService
{
    private readonly IWorkerInfoRepository _workerInfoRepo;

    public WorkerInfoServiceImpl(IWorkerInfoRepository workerInfoRepo)
    {
        _workerInfoRepo = workerInfoRepo;
    }

    public Task<WorkerInfo> GetOneAsync(Guid workerId, Guid workerInfoId, CancellationToken cancellationToken = default)
        => _workerInfoRepo.GetOneAsync(workerId, workerInfoId, cancellationToken);

    public async Task<List<WorkerInfo>> GetAllAsync(Guid workerId, CancellationToken cancellationToken = default)
    {
        var result = await _workerInfoRepo.GetAllAsync(workerId, cancellationToken)
            ?? throw new KeyNotFoundException();
        return result;
    }

    public Task<HashSet<WorkerInfo>> GetAllAsyncHash(Guid workerId, CancellationToken cancellationToken = default)
        => _workerInfoRepo.GetAllAsyncHash(workerId, cancellationToken);

    /// <summary>Insert a new worker_info row.</summary>
    public async Task<WorkerInfo> CreateAsync(Guid workerId, WorkerInfo workerInfo, CancellationToken cancellationToken = default)
    {
        await _workerInfoRepo.AddOneAsync(workerId, workerInfo, cancellationToken);
        return workerInfo;
    }

    public Task<WorkerInfo> UpdateAsync(Guid workerId, WorkerInfo workerInfo, CancellationToken cancellationToken = default)
        => _workerInfoRepo.UpdateAsync(workerId, workerInfo, cancellationToken);

    public Task<WorkerInfo> DeleteAsync(Guid workerId, Guid workerInfoId, CancellationToken cancellationToken = default)
        => _workerInfoRepo.DeleteAsync(workerId, workerInfoId, cancellationToken);

    /// <summary>Return all fields (preset + custom) for the profile page.</summary>
    public Task<List<WorkerInfo>> GetAllWithPresetsAsync(Guid workerId, CancellationToken cancellationToken = default)
        => _workerInfoRepo.GetAllWithPresetsAsync(workerId, cancellationToken);

    public Task<List<WorkerInfo>> GetEffectiveWorkerInfo(Guid workerId, Guid employerId, CancellationToken cancellationToken = default)
        => _workerInfoRepo.GetEffectiveWorkerInfo(workerId, employerId, cancellationToken);

    public Task<List<WorkerInfo>> GetRequestedWorkerInfos(Guid workerId, Guid employerId, CancellationToken cancellationToken = default)
        => _workerInfoRepo.GetRequestedWorkerInfos(workerId, employerId, cancellationToken);
}
