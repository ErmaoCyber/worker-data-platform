using Microsoft.EntityFrameworkCore;
using wdb_backend.Abstractions;
using wdb_backend.Data;
using wdb_backend.Models;

namespace wdb_backend.Services;

public class RequestRepoImpl : IRequestRepository
{
    private readonly AppDbContext _dbContext;

    public RequestRepoImpl(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Create a new request. ExpiryDate defaults to 90 days from now;
    /// callers can pass a custom value via the overload in Phase 2.
    /// </summary>
    public async Task<Request> AddAsync(
        Guid employerId,
        Guid workerId,
        string reason,
        CancellationToken cancellationToken = default)
    {
        var request = new Request
        {
            EmployerId = employerId,
            WorkerId = workerId,
            Reason = reason,
            ExpiryDate = DateTime.UtcNow.AddDays(90)   // default; will be overridable in Phase 2
        };

        _dbContext.Requests.Add(request);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return request;
    }

    public Task<LinkedList<Request>> GetAllByEmployerIdAsync(
        Guid employerId,
        CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public async Task<List<Request>> GetAllByWorkerIdAsync(
        Guid workerId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Requests
            .Where(x => x.WorkerId == workerId)
            .ToListAsync(cancellationToken);
    }

    public async Task<Request> GetByRequestIdAsync(
        Guid requestId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Requests
            .FirstOrDefaultAsync(x => x.Id == requestId, cancellationToken)
            ?? throw new KeyNotFoundException();
    }
}
