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

    /// <summary>
    /// Create one permission per WorkerInfo item under a request.
    /// This still supports the old existing-info request flow.
    /// For new preset field-first requests, a later employer-side update should create
    /// permissions with field_id and info_id = null.
    /// </summary>
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

    /// <summary>
    /// Create a permission linked to an existing worker_info row.
    /// </summary>
    public async Task AddOneByRequestAsync(
        Request request,
        WorkerInfo workerInfo,
        CancellationToken cancellationToken = default)
    {
        var permission = new Permission
        {
            InfoId = workerInfo.Id,
            FieldId = workerInfo.FieldId,
            RequestId = request.Id,
            WorkerId = workerInfo.WorkerId,
            Status = PermissionStatus.Pending,
            LastUpdatedAt = DateTime.UtcNow
        };

        _dbContext.Permissions.Add(permission);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Persist permission changes.
    /// </summary>
    public async Task<Permission> UpdateAsync(
        Guid permissionId,
        Permission permission,
        CancellationToken cancellationToken = default)
    {
        var item = await _dbContext.Permissions
            .FirstOrDefaultAsync(x => x.Id == permissionId, cancellationToken)
            ?? throw new KeyNotFoundException();

        item.FieldId = permission.FieldId;
        item.InfoId = permission.InfoId;
        item.Status = permission.Status;
        item.LastUpdatedAt = permission.LastUpdatedAt;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return await GetOneAsync(permissionId, cancellationToken);
    }

    public async Task<List<Permission>> GetAllByRequestIdAsync(
        Guid requestId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Permissions
            .Where(x => x.RequestId == requestId)
            .Include(p => p.Request)
            .Include(p => p.Field)
                .ThenInclude(f => f!.Category)
            .Include(p => p.WorkerInfo)
                .ThenInclude(w => w!.Field)
                    .ThenInclude(f => f!.Category)
            .ToListAsync(cancellationToken);
    }

    public async Task<Permission> GetOneAsync(
        Guid permissionId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Permissions
            .Include(p => p.Request)
            .Include(p => p.Field)
                .ThenInclude(f => f!.Category)
            .Include(p => p.WorkerInfo)
                .ThenInclude(w => w!.Field)
                    .ThenInclude(f => f!.Category)
            .FirstOrDefaultAsync(x => x.Id == permissionId, cancellationToken)
            ?? throw new KeyNotFoundException();
    }

    public async Task<List<Permission>> GetAllByWorkerIdAsync(
        Guid workerId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Permissions
            .Where(x => x.WorkerId == workerId)
            .Include(p => p.Request)
            .Include(p => p.Field)
                .ThenInclude(f => f!.Category)
            .Include(p => p.WorkerInfo)
                .ThenInclude(w => w!.Field)
                    .ThenInclude(f => f!.Category)
            .ToListAsync(cancellationToken);
    }
}
