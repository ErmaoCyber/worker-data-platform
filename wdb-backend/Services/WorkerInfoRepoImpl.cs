using Microsoft.EntityFrameworkCore;
using wdb_backend.Abstractions;
using wdb_backend.Common;
using wdb_backend.Data;
using wdb_backend.Models;

namespace wdb_backend.Services;

public class WorkerInfoRepoImpl : IWorkerInfoRepository
{
    private readonly AppDbContext _context;

    public WorkerInfoRepoImpl(AppDbContext context)
    {
        _context = context;
    }

    public Task<WorkerInfo> GetOneAsync(Guid workerId, Guid workerInfoId, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    /// <summary>Return all worker_info rows for a worker, including Field + Category navigation.</summary>
    public async Task<List<WorkerInfo>> GetAllAsync(Guid workerId, CancellationToken cancellationToken = default)
    {
        return await _context.WorkerInfos
            .Where(w => w.WorkerId == workerId)
            .Include(w => w.Field)
                .ThenInclude(f => f!.Category)
            .ToListAsync(cancellationToken);
    }

    public async Task<HashSet<WorkerInfo>> GetAllAsyncHash(Guid workerId, CancellationToken cancellationToken = default)
    {
        var list = await GetAllAsync(workerId, cancellationToken);
        return list.ToHashSet();
    }

    /// <summary>Insert a new worker_info row.</summary>
    public Task AddOneAsync(Guid workerId, WorkerInfo workerInfo, CancellationToken cancellationToken = default)
    {
        workerInfo.WorkerId = workerId;
        _context.WorkerInfos.Add(workerInfo);
        return _context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Update an existing worker_info row matched by WorkerId + FieldId (preset)
    /// or WorkerId + CustomLabel (custom). Only Value is updated for preset fields;
    /// Label and Value can both be updated for custom fields.
    /// </summary>
    public async Task<WorkerInfo> UpdateAsync(Guid workerId, WorkerInfo workerInfo, CancellationToken cancellationToken = default)
    {
        WorkerInfo? existing;

        if (workerInfo.FieldId.HasValue)
        {
            // Preset field — match by FieldId
            existing = await _context.WorkerInfos
                .FirstOrDefaultAsync(w => w.WorkerId == workerId && w.FieldId == workerInfo.FieldId, cancellationToken);
        }
        else
        {
            // Custom field — match by Id directly
            existing = await _context.WorkerInfos
                .FirstOrDefaultAsync(w => w.WorkerId == workerId && w.Id == workerInfo.Id, cancellationToken);
        }

        if (existing != null)
        {
            existing.Value = workerInfo.Value;
            existing.UpdatedAt = DateTime.UtcNow;

            // For custom fields, label can also be updated
            if (!workerInfo.FieldId.HasValue && workerInfo.CustomLabel != null)
                existing.CustomLabel = workerInfo.CustomLabel;

            await _context.SaveChangesAsync(cancellationToken);
            return existing;
        }

        // Row does not exist yet — insert (preset field first fill)
        workerInfo.WorkerId = workerId;
        _context.WorkerInfos.Add(workerInfo);
        await _context.SaveChangesAsync(cancellationToken);
        return workerInfo;
    }

    /// <summary>
    /// Delete a custom worker_info row.
    /// Returns the deleted row, or throws InvalidOperationException if
    /// the row has an active (approved) permission.
    /// </summary>
    public async Task<WorkerInfo> DeleteAsync(Guid workerId, Guid workerInfoId, CancellationToken cancellationToken = default)
    {
        var existing = await _context.WorkerInfos
            .Include(w => w.Permissions)
            .FirstOrDefaultAsync(w => w.Id == workerInfoId && w.WorkerId == workerId, cancellationToken)
            ?? throw new KeyNotFoundException();

        // Block deletion if there is any active (approved) permission
        var hasActive = existing.Permissions.Any(p => p.Status == PermissionStatus.Approved);
        if (hasActive)
            throw new InvalidOperationException("ACTIVE_PERMISSION_EXISTS");

        _context.WorkerInfos.Remove(existing);
        await _context.SaveChangesAsync(cancellationToken);
        return existing;
    }

    /// <summary>
    /// Return all preset fields (from the fields table) merged with the worker's
    /// existing worker_info rows. Fields with no worker_info row are included with Value=null.
    /// Also includes all custom (Other) fields the worker has created.
    /// Result is grouped by category for the profile page.
    /// </summary>
    public async Task<List<WorkerInfo>> GetAllWithPresetsAsync(Guid workerId, CancellationToken cancellationToken = default)
    {
        // All preset field definitions
        var allFields = await _context.Fields
            .Include(f => f.Category)
            .ToListAsync(cancellationToken);

        // Worker's existing rows (preset + custom)
        var existing = await _context.WorkerInfos
            .Where(w => w.WorkerId == workerId)
            .Include(w => w.Field)
                .ThenInclude(f => f!.Category)
            .ToListAsync(cancellationToken);

        var existingByFieldId = existing
            .Where(w => w.FieldId.HasValue)
            .ToDictionary(w => w.FieldId!.Value);

        var result = new List<WorkerInfo>();

        // For each preset field, use the existing row or create a placeholder
        foreach (var field in allFields)
        {
            if (existingByFieldId.TryGetValue(field.Id, out var row))
            {
                result.Add(row);
            }
            else
            {
                // Placeholder — not yet filled
                result.Add(new WorkerInfo
                {
                    Id = Guid.Empty,   // signals "not yet saved"
                    WorkerId = workerId,
                    FieldId = field.Id,
                    Field = field,
                    Type = field.AllowedType,
                    Value = null,
                    CustomLabel = null
                });
            }
        }

        // Append custom (Other) fields
        var customFields = existing.Where(w => !w.FieldId.HasValue);
        result.AddRange(customFields);

        return result;
    }

    /// <summary>
    /// Return worker infos not yet requested by this employer.
    /// Uses int status constants (PermissionStatus.Pending, .Approved).
    /// </summary>
    public async Task<List<WorkerInfo>> GetEffectiveWorkerInfo(Guid workerId, Guid employerId, CancellationToken cancellationToken = default)
    {
        var availableInfos = await _context.WorkerInfos
            .Include(w => w.Permissions).ThenInclude(p => p.Request)
            .Include(w => w.Field).ThenInclude(f => f!.Category)
            .Where(w => w.WorkerId == workerId &&
                        !w.Permissions.Any(p =>
                            p.Request.EmployerId == employerId &&
                            (p.Status == PermissionStatus.Pending ||
                             p.Status == PermissionStatus.Approved)))
            .ToListAsync(cancellationToken);

        return availableInfos;
    }

    /// <summary>Return worker infos already requested by this employer.</summary>
    public async Task<List<WorkerInfo>> GetRequestedWorkerInfos(Guid workerId, Guid employerId, CancellationToken cancellationToken = default)
    {
        var requestedInfos = await _context.WorkerInfos
            .Include(w => w.Permissions).ThenInclude(p => p.Request)
            .Include(w => w.Field).ThenInclude(f => f!.Category)
            .Where(w => w.WorkerId == workerId &&
                        w.Permissions.Any(p =>
                            p.Request.EmployerId == employerId &&
                            (p.Status == PermissionStatus.Pending ||
                             p.Status == PermissionStatus.Approved)))
            .ToListAsync(cancellationToken);

        return requestedInfos;
    }
}
