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

    /// <summary>
    /// Bulk-create permissions for selected worker info rows.
    /// Note: This method still supports the older flow where info_id is set immediately.
    /// The new worker review flow can also handle field_id-first permissions.
    /// </summary>
    public async Task CreateAllByRequestAsync(
        Request request,
        List<WorkerInfo> workerInfos,
        CancellationToken cancellationToken = default)
    {
        await _permissionRepository.AddAllByRequestAsync(
            request,
            workerInfos,
            cancellationToken);
    }

    /// <summary>
    /// Update permission status.
    /// This legacy method does not resolve info_id for preset approval.
    /// Prefer WorkerRequestReviewServiceImpl for worker review submission.
    /// </summary>
    public async Task<Permission> UpdateAsync(
        Guid permissionId,
        int status,
        CancellationToken cancellationToken = default)
    {
        var permission = await _permissionRepository.GetOneAsync(
            permissionId,
            cancellationToken);

        if (permission.Status == PermissionStatus.Rejected ||
            permission.Status == PermissionStatus.Revoked)
        {
            throw new InvalidOperationException(
                $"Permission {permissionId} is in a terminal state and cannot be updated.");
        }

        permission.Status = status;
        permission.LastUpdatedAt = DateTime.UtcNow;

        return await _permissionRepository.UpdateAsync(
            permissionId,
            permission,
            cancellationToken);
    }

    public async Task<List<Permission>> GetAllByWorkerIdAsync(
        Guid workerId,
        int status = -1,
        CancellationToken cancellationToken = default)
    {
        var result = await _permissionRepository.GetAllByWorkerIdAsync(
            workerId,
            cancellationToken);

        return status == -1
            ? result
            : result.Where(x => x.Status == status).ToList();
    }

    public async Task<IReadOnlyList<Permission>> GetAllByRequestIdAsync(
        Guid requestId,
        CancellationToken cancellationToken = default)
    {
        return await _permissionRepository.GetAllByRequestIdAsync(
            requestId,
            cancellationToken);
    }

    public async Task<Permission> GetByIdAsync(
        Guid permissionId,
        CancellationToken cancellationToken = default)
    {
        return await _permissionRepository.GetOneAsync(
            permissionId,
            cancellationToken);
    }
}
