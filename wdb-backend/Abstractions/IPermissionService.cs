using wdb_backend.Models;

namespace wdb_backend.Abstractions;

public interface IPermissionService
{
    /// <summary>Bulk-create permissions for a list of WorkerInfo rows under a request.</summary>
    Task CreateAllByRequestAsync(Request request, List<WorkerInfo> workerInfos, CancellationToken cancellationToken = default);

    /// <summary>Update status of a single permission. Status is an int (0-3).</summary>
    Task<Permission> UpdateAsync(Guid permissionId, int status, CancellationToken cancellationToken = default);

    /// <summary>Get all permissions for a worker. Pass status=-1 to get all regardless of status.</summary>
    Task<List<Permission>> GetAllByWorkerIdAsync(Guid workerId, int status = -1, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Permission>> GetAllByRequestIdAsync(Guid requestId, CancellationToken cancellationToken = default);
    Task<Permission> GetByIdAsync(Guid permissionId, CancellationToken cancellationToken = default);
}
