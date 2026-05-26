using wdb_backend.Dtos;

namespace wdb_backend.Abstractions;

/// <summary>
/// Provides audit log data for workers.
/// The implementation will read blockchain records and convert them into frontend-friendly DTOs.
/// </summary>
public interface IWorkerAuditLogService
{
    /// <summary>
    /// Gets the audit log records for a worker.
    /// </summary>
    /// <param name="workerId">
    /// The worker ID from the application database.
    /// </param>
    /// <returns>
    /// A response DTO containing the worker ID and their audit log records.
    /// </returns>
    Task<WorkerAuditLogResponseDto> GetWorkerAuditLogAsync(Guid workerId);
}
