using wdb_backend.DTOs;

namespace wdb_backend.Abstractions;

public interface IWorkerDashboardService
{
    Task<WorkerDashboardResponseDto?> GetDashboardAsync(
        Guid workerId,
        CancellationToken cancellationToken = default
    );
}
