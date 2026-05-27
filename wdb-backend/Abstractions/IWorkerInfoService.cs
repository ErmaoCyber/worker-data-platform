using wdb_backend.Models;

namespace wdb_backend.Abstractions;

public interface IWorkerInfoService
{
    Task<WorkerInfo> GetOneAsync(Guid workerId, Guid workerInfoId, CancellationToken cancellationToken = default);

    Task<List<WorkerInfo>> GetAllAsync(Guid workerId, CancellationToken cancellationToken = default);
    Task<HashSet<WorkerInfo>> GetAllAsyncHash(Guid workerId, CancellationToken cancellationToken = default);

    Task<WorkerInfo> CreateAsync(Guid workerId, WorkerInfo workerInfo, CancellationToken cancellationToken = default);

    Task<WorkerInfo> UpdateAsync(Guid workerId, WorkerInfo workerInfo, CancellationToken cancellationToken = default);

    Task<WorkerInfo> DeleteAsync(Guid workerId, Guid workerInfoId, CancellationToken cancellationToken = default);
    Task<List<WorkerInfo>> GetEffectiveWorkerInfo(Guid workerId, Guid employerId, CancellationToken cancellationToken);

    Task<List<WorkerInfo>> GetRequestedWorkerInfos(Guid workerId, Guid employerId, CancellationToken cancellationToken);
    Dictionary<string, List<string>> GetCategoryFields();
}
