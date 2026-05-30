namespace wdb_backend.Abstractions;

public interface ICreateDataAccessRequestUsecase
{
    /// <summary>
    /// Create one request and one permission per selected item.
    /// selectedItemIds:
    /// - preset field id
    /// - or custom worker_info id
    /// customRequest:
    /// - optional free-text request for new information that does not exist yet
    /// </summary>
    Task CreateDataAccessRequest(
        List<Guid> selectedItemIds,
        Guid employerId,
        Guid workerId,
        string reason,
        string? customRequest = null,
        CancellationToken cancellationToken = default);
}
