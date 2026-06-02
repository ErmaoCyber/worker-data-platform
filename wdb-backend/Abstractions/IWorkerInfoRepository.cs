using wdb_backend.Models;

namespace wdb_backend.Abstractions;

public interface IWorkerInfoRepository
{
    Task<WorkerInfo> GetOneAsync(Guid workerId, Guid workerInfoId, CancellationToken cancellationToken = default);
    Task<List<WorkerInfo>> GetAllAsync(Guid workerId, CancellationToken cancellationToken = default);
    Task<HashSet<WorkerInfo>> GetAllAsyncHash(Guid workerId, CancellationToken cancellationToken = default);
    Task AddOneAsync(Guid workerId, WorkerInfo workerInfo, CancellationToken cancellationToken = default);
    Task<WorkerInfo> UpdateAsync(Guid workerId, WorkerInfo workerInfo, CancellationToken cancellationToken = default);
    Task<WorkerInfo> DeleteAsync(Guid workerId, Guid workerInfoId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Return all preset fields merged with existing worker_info rows (unfilled = Value null),
    /// plus all custom (Other) fields. Used for the profile page.
    /// </summary>
    Task<List<WorkerInfo>> GetAllWithPresetsAsync(Guid workerId, CancellationToken cancellationToken = default);

    Task<List<WorkerInfo>> GetEffectiveWorkerInfo(Guid workerId, Guid employerId, CancellationToken cancellationToken);
    Task<List<WorkerInfo>> GetRequestedWorkerInfos(Guid workerId, Guid employerId, CancellationToken cancellationToken);
}
