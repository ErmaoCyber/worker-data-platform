using wdb_backend.Abstractions;
using wdb_backend.Models;

namespace wdb_backend.Services;

<<<<<<< HEAD:wdb-backend/Services/EmployerServicerImpl.cs
public class EmployerServicerImpl : IEmployerService

=======
public class EmployerServiceImpl : IEmployerService
>>>>>>> origin:wdb-backend/Services/EmployerServiceImpl.cs
{
    private readonly IEmployerRepository _employerRepository;

    public EmployerServiceImpl(IEmployerRepository employerRepository)
    {
        _employerRepository = employerRepository;
    }

    public Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Employer> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Employer> CreateAsync(Employer employer, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Employer> UpdateAsync(string email, Employer employer, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Employer> DeleteAsync(string email, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<Employer?> GetEmployerInfoAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _employerRepository.GetByIdAsync(id, cancellationToken);
    }

    public async Task<List<Employer>> GetDistinctEmployers(CancellationToken cancellationToken = default)
    {
        return await _employerRepository.GetDistinctEmployers(cancellationToken);
    }
}

