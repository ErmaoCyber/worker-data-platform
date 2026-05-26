using wdb_backend.DTOs;

namespace wdb_backend.Abstractions;

public interface IActiveAccessService
{
    Task<List<ActiveAccessDto>> GetActiveAccessAsync(
        Guid workerId,
        string? company = null,
        string? dataType = null);
}
