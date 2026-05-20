namespace wdb_backend.Abstractions;


public interface IAddFlexibleWorkerInfoUsecase
{
    Task ExecuteAsync(string workerEmail, string category, string desc, Guid employerId, CancellationToken cancellationToken = default);

}