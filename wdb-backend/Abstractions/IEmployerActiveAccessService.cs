using wdb_backend.DTOs;

namespace wdb_backend.Abstractions;

public interface IEmployerActiveAccessService
{
    Task<List<EmployerActiveAccessDto>> GetActiveAccessAsync(
        Guid employerId,
        CancellationToken cancellationToken = default);
}
