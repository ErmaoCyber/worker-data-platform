namespace wdb_backend.Abstractions;

public interface ICreateDataAccessRequestUsecase
{
    /// <summary>
    /// Create one request and one permission per selected item.
    /// selectedItemIds:
    /// - preset field id
    /// - or custom worker_info id
    /// </summary>
    Task CreateDataAccessRequest(
        List<Guid> selectedItemIds,
        Guid employerId,
        Guid workerId,
        string reason,
        CancellationToken cancellationToken = default);
}
