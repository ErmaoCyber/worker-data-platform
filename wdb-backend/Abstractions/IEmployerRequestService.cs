using wdb_backend.DTOs;

namespace wdb_backend.Abstractions;

public interface IEmployerRequestService
{
    // Returns null when the target worker email does not exist.
    Task<EmployerRequestCatalogDto?> GetCatalogAsync(
        string workerEmail,
        CancellationToken cancellationToken = default);

    Task<CreateEmployerRequestResultDto> CreateAsync(
        Guid employerId,
        CreateEmployerRequestDto dto,
        CancellationToken cancellationToken = default);
}
