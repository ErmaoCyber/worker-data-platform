using wdb_backend.DTOs;

namespace wdb_backend.Abstractions;

public interface IActiveAccessService
{
    Task<List<ActiveAccessDto>> GetActiveAccessAsync(
        Guid workerId,
        string? company = null,
        string? dataType = null);

    /// <summary>
    /// Revoke all approved permissions under one active request/access grant.
    /// This is request-level revoke, not single item revoke.
    /// </summary>
    Task RevokeRequestAccessAsync(
        Guid workerId,
        Guid requestId,
        CancellationToken cancellationToken = default);
}
