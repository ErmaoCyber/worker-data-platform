using wdb_backend.Models;

namespace wdb_backend.Abstractions;

public interface IPermissionRepository
{
    /// <summary>Bulk-create one permission row per WorkerInfo under the given request.</summary>
    Task AddAllByRequestAsync(Request request, List<WorkerInfo> workerInfos, CancellationToken cancellationToken = default);

    /// <summary>Update a single permission row.</summary>
    Task<Permission> UpdateAsync(Guid permissionId, Permission permission, CancellationToken cancellationToken = default);

    /// <summary>Get all permissions belonging to a request.</summary>
    Task<List<Permission>> GetAllByRequestIdAsync(Guid requestId, CancellationToken cancellationToken = default);

    /// <summary>Get a single permission by its own ID.</summary>
    Task<Permission> GetOneAsync(Guid permissionId, CancellationToken cancellationToken = default);

    /// <summary>Get all permissions for a worker, optionally filtered by status int value.</summary>
    Task<List<Permission>> GetAllByWorkerIdAsync(Guid workerId, CancellationToken cancellationToken = default);

    /// <summary>Create one permission row linked to a request and a WorkerInfo row.</summary>
    Task AddOneByRequestAsync(Request request, WorkerInfo workerInfo, CancellationToken cancellationToken = default);
}
