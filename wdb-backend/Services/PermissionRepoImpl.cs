using Microsoft.EntityFrameworkCore;
using wdb_backend.Abstractions;
using wdb_backend.Common;
using wdb_backend.Data;
using wdb_backend.Models;

namespace wdb_backend.Services;

public class PermissionRepoImpl : IPermissionRepository
{
    private readonly AppDbContext _dbContext;

    public PermissionRepoImpl(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>Create one permission per WorkerInfo item under a request.</summary>
    public async Task AddAllByRequestAsync(
        Request request,
        List<WorkerInfo> workerInfos,
        CancellationToken cancellationToken = default)
    {
        foreach (var workerInfo in workerInfos)
        {
            await AddOneByRequestAsync(request, workerInfo, cancellationToken);
        }
    }

    /// <summary>Create a single permission row. info_id is set immediately (existing data rows).</summary>
    public async Task AddOneByRequestAsync(
        Request request,
        WorkerInfo workerInfo,
        CancellationToken cancellationToken = default)
    {
        var permission = new Permission
        {
            InfoId = workerInfo.Id,   // nullable — set here because the row already exists
            RequestId = request.Id,
            WorkerId = workerInfo.WorkerId,
            Status = PermissionStatus.Pending
        };

        _dbContext.Permissions.Add(permission);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <summary>Persist status and timestamp changes to a permission.</summary>
    public async Task<Permission> UpdateAsync(
        Guid permissionId,
        Permission permission,
        CancellationToken cancellationToken = default)
    {
        var item = await _dbContext.Permissions
            .FirstOrDefaultAsync(x => x.Id == permissionId, cancellationToken)
            ?? throw new KeyNotFoundException();

        item.Status = permission.Status;
        item.LastUpdatedAt = permission.LastUpdatedAt;

        await _dbContext.SaveChangesAsync(cancellationToken);
        return item;
    }

    /// <summary>Get all permissions for a request.</summary>
    public async Task<List<Permission>> GetAllByRequestIdAsync(
        Guid requestId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Permissions
            .Where(x => x.RequestId == requestId)
            .ToListAsync(cancellationToken);
    }

    /// <summary>Get a single permission by ID.</summary>
    public async Task<Permission> GetOneAsync(
        Guid permissionId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Permissions
            .FirstOrDefaultAsync(x => x.Id == permissionId, cancellationToken)
            ?? throw new KeyNotFoundException();
    }

    /// <summary>Get all permissions for a worker.</summary>
    public async Task<List<Permission>> GetAllByWorkerIdAsync(
        Guid workerId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Permissions
            .Where(x => x.WorkerId == workerId)
            .ToListAsync(cancellationToken);
    }
}
