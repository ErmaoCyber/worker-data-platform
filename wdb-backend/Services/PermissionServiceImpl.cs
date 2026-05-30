using wdb_backend.Abstractions;
using wdb_backend.Common;
using wdb_backend.Models;

namespace wdb_backend.Services;

public class PermissionServiceImpl : IPermissionService
{
    private readonly IPermissionRepository _permissionRepository;

    public PermissionServiceImpl(IPermissionRepository permissionRepository)
    {
        _permissionRepository = permissionRepository;
    }

    /// <summary>Bulk-create permissions for all selected WorkerInfo rows.</summary>
    public async Task CreateAllByRequestAsync(
        Request request,
        List<WorkerInfo> workerInfos,
        CancellationToken cancellationToken = default)
    {
        await _permissionRepository.AddAllByRequestAsync(request, workerInfos, cancellationToken);
    }

    /// <summary>
    /// Update permission status (0=Pending,1=Approved,2=Rejected,3=Revoked).
    /// Throws InvalidOperationException if the permission is already in a terminal state.
    /// </summary>
    public async Task<Permission> UpdateAsync(
        Guid permissionId,
        int status,
        CancellationToken cancellationToken = default)
    {
        var permission = await _permissionRepository.GetOneAsync(permissionId, cancellationToken)
            ?? throw new KeyNotFoundException();

        // Terminal states cannot be changed
        if (permission.Status == PermissionStatus.Rejected ||
            permission.Status == PermissionStatus.Revoked)
        {
            throw new InvalidOperationException(
                $"Permission {permissionId} is in a terminal state and cannot be updated.");
        }

        permission.Status = status;
        permission.LastUpdatedAt = DateTime.UtcNow;

        return await _permissionRepository.UpdateAsync(permissionId, permission, cancellationToken);
    }

    /// <summary>
    /// Get all permissions for a worker. Pass status=-1 to skip filtering.
    /// </summary>
    public async Task<List<Permission>> GetAllByWorkerIdAsync(
        Guid workerId,
        int status = -1,
        CancellationToken cancellationToken = default)
    {
        var result = await _permissionRepository.GetAllByWorkerIdAsync(workerId, cancellationToken)
            ?? throw new KeyNotFoundException();

        return status == -1
            ? result
            : result.Where(x => x.Status == status).ToList();
    }

    public Task<IReadOnlyList<Permission>> GetAllByRequestIdAsync(
        Guid requestId,
        CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task<Permission> GetByIdAsync(
        Guid permissionId,
        CancellationToken cancellationToken = default)
        => throw new NotImplementedException();
}
