using wdb_backend.DTOs;

namespace wdb_backend.Abstractions;

public interface IWorkerRequestReviewService
{
    /// <summary>
    /// Return all active pending requests for the current worker.
    /// </summary>
    Task<List<WorkerActiveRequestDto>> GetActiveRequestsAsync(
        Guid workerId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Submit approve/reject decisions for one request.
    /// </summary>
    Task SubmitReviewAsync(
        Guid workerId,
        Guid requestId,
        SubmitWorkerRequestReviewRequest request,
        CancellationToken cancellationToken = default);
}
