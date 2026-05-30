using wdb_backend.DTOs;

namespace wdb_backend.Abstractions;

public interface IActiveAccessService
{
    Task<List<ActiveAccessDto>> GetActiveAccessAsync(
        Guid workerId,
        string? company = null,
        string? dataType = null);

    /// <summary>
    /// Revoke one approved active permission owned by the worker.
    /// </summary>
    Task RevokePermissionAsync(
        Guid workerId,
        Guid permissionId,
        CancellationToken cancellationToken = default);
}
