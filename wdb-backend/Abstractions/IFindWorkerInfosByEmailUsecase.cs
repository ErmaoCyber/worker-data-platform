using System.Security.Cryptography.X509Certificates;
using wdb_backend.Models;

public interface IFindWorkerInfosByEmailUsecase
{

    Task<List<WorkerInfo>> FindWorkerInfosByEmail(string email,Guid employerId, CancellationToken cancellationToken = default);
    Task<List<WorkerInfo>> FindRequestedWorkerInfosByEmail(string email, Guid employerId, CancellationToken cancellationToken = default);

}
