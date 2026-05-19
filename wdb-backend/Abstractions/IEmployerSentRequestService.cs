using wdb_backend.DTOs;

namespace wdb_backend.Abstractions;

public interface IEmployerSentRequestService
{
    Task<List<EmployerSentRequestDto>> GetSentRequestsAsync(
        Guid employerId,
        CancellationToken cancellationToken = default);
}
