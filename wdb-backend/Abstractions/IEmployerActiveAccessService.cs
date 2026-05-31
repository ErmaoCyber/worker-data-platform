using wdb_backend.DTOs;

namespace wdb_backend.Abstractions;

public interface IEmployerActiveAccessService
{
    Task<List<EmployerActiveAccessDto>> GetActiveAccessAsync(
        Guid employerId,
        CancellationToken cancellationToken = default);

    // E4 view: validates ownership/status/expiry and returns text inline
    // or a short-lived Supabase signed URL for file items.
    Task<EmployerAccessViewResultDto> ViewAsync(
        Guid employerId,
        Guid permissionId,
        CancellationToken cancellationToken = default);
}
