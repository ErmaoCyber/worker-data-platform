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

    /// <summary>
    /// Return one worker_info row owned by the worker.
    /// Includes Field and Category so the caller can build display labels.
    /// </summary>
    public async Task<WorkerInfo> GetOneAsync(
        Guid workerId,
        Guid workerInfoId,
        CancellationToken cancellationToken = default)
    {
        return await _context.WorkerInfos
            .Include(w => w.Field)
                .ThenInclude(f => f!.Category)
            .FirstOrDefaultAsync(
                w => w.WorkerId == workerId && w.Id == workerInfoId,
                cancellationToken)
            ?? throw new KeyNotFoundException();
    }

    /// <summary>
    /// Return all saved worker_info rows for a worker.
    /// This does not include unsaved preset placeholders.
    /// </summary>
    public async Task<List<WorkerInfo>> GetAllAsync(
        Guid workerId,
        CancellationToken cancellationToken = default)
    {
        return await _context.WorkerInfos
            .Where(w => w.WorkerId == workerId)
            .Include(w => w.Field)
                .ThenInclude(f => f!.Category)
            .OrderBy(w => w.Field != null ? w.Field.Category.CategoryName : "OtherInformation")
            .ThenBy(w => w.Field != null ? w.Field.Label : w.CustomLabel)
            .ToListAsync(cancellationToken);
    }

    public async Task<HashSet<WorkerInfo>> GetAllAsyncHash(
        Guid workerId,
        CancellationToken cancellationToken = default)
    {
        var list = await GetAllAsync(workerId, cancellationToken);
        return list.ToHashSet();
    }

    /// <summary>
    /// Insert a new worker_info row.
    /// In the worker profile flow this is mainly used for custom OtherInformation fields.
    /// </summary>
    public async Task AddOneAsync(
        Guid workerId,
        WorkerInfo workerInfo,
        CancellationToken cancellationToken = default)
    {
        workerInfo.WorkerId = workerId;

        if (workerInfo.FieldId.HasValue)
        {
            await PreparePresetForInsertAsync(workerInfo, cancellationToken);
        }
        else
        {
            await PrepareCustomForInsertAsync(workerId, workerInfo, cancellationToken);
        }

        _context.WorkerInfos.Add(workerInfo);
        await _context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Update worker_info.
    /// Preset fields are upserted by WorkerId + FieldId.
    /// Custom fields are updated by WorkerId + Id.
    /// </summary>
    public async Task<WorkerInfo> UpdateAsync(
        Guid workerId,
        WorkerInfo workerInfo,
        CancellationToken cancellationToken = default)
    {
        if (workerInfo.FieldId.HasValue)
        {
            return await UpsertPresetFieldAsync(workerId, workerInfo, cancellationToken);
        }

        return await UpdateCustomFieldAsync(workerId, workerInfo, cancellationToken);
    }

    /// <summary>
    /// Delete a custom worker_info row.
    /// Preset fields cannot be deleted.
    /// If there is no permission history, the row is physically deleted.
    /// If there is active approved access, deletion is blocked until the worker revokes access.
    /// If there is any non-active permission history, deletion is blocked to preserve audit history.
    /// </summary>
    public async Task<WorkerInfo> DeleteAsync(
        Guid workerId,
        Guid workerInfoId,
        CancellationToken cancellationToken = default)
    {
        var existing = await _context.WorkerInfos
            .Include(w => w.Permissions)
            .FirstOrDefaultAsync(
                w => w.Id == workerInfoId && w.WorkerId == workerId,
                cancellationToken)
            ?? throw new KeyNotFoundException();

        if (existing.FieldId.HasValue)
            throw new InvalidOperationException("PRESET_FIELD_CANNOT_BE_DELETED");

        var hasActivePermission = existing.Permissions
            .Any(p => p.Status == PermissionStatus.Approved);

        if (hasActivePermission)
            throw new InvalidOperationException("ACTIVE_PERMISSION_EXISTS");

        var hasPermissionHistory = existing.Permissions.Any();

        if (hasPermissionHistory)
            throw new InvalidOperationException("PERMISSION_HISTORY_EXISTS");

        _context.WorkerInfos.Remove(existing);
        await _context.SaveChangesAsync(cancellationToken);

        return existing;
    }

    /// <summary>
    /// Return all preset fields merged with the worker's saved worker_info rows.
    /// Preset fields with no saved value are returned as placeholders.
    /// Custom fields are appended under OtherInformation.
    /// </summary>
    public async Task<List<WorkerInfo>> GetAllWithPresetsAsync(
        Guid workerId,
        CancellationToken cancellationToken = default)
    {
        var allPresetFields = await _context.Fields
            .Include(f => f.Category)
            .OrderBy(f => f.Category.CategoryName)
            .ThenBy(f => f.Label)
            .ToListAsync(cancellationToken);

        var existingRows = await _context.WorkerInfos
            .Where(w => w.WorkerId == workerId)
            .Include(w => w.Field)
                .ThenInclude(f => f!.Category)
            .ToListAsync(cancellationToken);

        var existingByFieldId = existingRows
            .Where(w => w.FieldId.HasValue)
            .ToDictionary(w => w.FieldId!.Value);

        var result = new List<WorkerInfo>();

        foreach (var field in allPresetFields)
        {
            if (existingByFieldId.TryGetValue(field.Id, out var existingRow))
            {
                result.Add(existingRow);
                continue;
            }

            // Placeholder only. This row is not saved in the database yet.
            result.Add(new WorkerInfo
            {
                Id = Guid.Empty,
                WorkerId = workerId,
                FieldId = field.Id,
                Field = field,
                CustomLabel = null,
                Type = field.AllowedType,
                Value = null
            });
        }

        var customFields = existingRows
            .Where(w => !w.FieldId.HasValue)
            .OrderBy(w => w.CustomLabel)
            .ToList();

        result.AddRange(customFields);

        return result;
    }

    /// <summary>
    /// Return worker_info rows that are available for this employer to request.
    /// This currently returns saved worker_info rows only.
    /// Preset fields that have not been filled yet are not requestable here.
    /// </summary>
    public async Task<List<WorkerInfo>> GetEffectiveWorkerInfo(
        Guid workerId,
        Guid employerId,
        CancellationToken cancellationToken = default)
    {
        var availableInfos = await _context.WorkerInfos
            .Include(w => w.Permissions)
                .ThenInclude(p => p.Request)
            .Include(w => w.Field)
                .ThenInclude(f => f!.Category)
            .Where(w =>
                w.WorkerId == workerId &&
                !w.Permissions.Any(p =>
                    p.Request.EmployerId == employerId &&
                    (p.Status == PermissionStatus.Pending ||
                     p.Status == PermissionStatus.Approved)))
            .OrderBy(w => w.Field != null ? w.Field.Category.CategoryName : "OtherInformation")
            .ThenBy(w => w.Field != null ? w.Field.Label : w.CustomLabel)
            .ToListAsync(cancellationToken);

        return availableInfos;
    }

    /// <summary>
    /// Return worker_info rows already requested by this employer.
    /// </summary>
    public async Task<List<WorkerInfo>> GetRequestedWorkerInfos(
        Guid workerId,
        Guid employerId,
        CancellationToken cancellationToken = default)
    {
        var requestedInfos = await _context.WorkerInfos
            .Include(w => w.Permissions)
                .ThenInclude(p => p.Request)
            .Include(w => w.Field)
                .ThenInclude(f => f!.Category)
            .Where(w =>
                w.WorkerId == workerId &&
                w.Permissions.Any(p =>
                    p.Request.EmployerId == employerId &&
                    (p.Status == PermissionStatus.Pending ||
                     p.Status == PermissionStatus.Approved)))
            .OrderBy(w => w.Field != null ? w.Field.Category.CategoryName : "OtherInformation")
            .ThenBy(w => w.Field != null ? w.Field.Label : w.CustomLabel)
            .ToListAsync(cancellationToken);

        return requestedInfos;
    }

    // ── Private helpers ───────────────────────────────────────────────────

    /// <summary>
    /// Prepare a preset worker_info row before insert.
    /// Type must come from fields.allowed_type.
    /// </summary>
    private async Task PreparePresetForInsertAsync(
        WorkerInfo workerInfo,
        CancellationToken cancellationToken)
    {
        var field = await _context.Fields
            .FirstOrDefaultAsync(f => f.Id == workerInfo.FieldId, cancellationToken)
            ?? throw new KeyNotFoundException("FIELD_NOT_FOUND");

        workerInfo.CustomLabel = null;
        workerInfo.Type = field.AllowedType;
        workerInfo.UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Prepare a custom worker_info row before insert.
    /// Custom label must be unique per worker.
    /// </summary>
    private async Task PrepareCustomForInsertAsync(
        Guid workerId,
        WorkerInfo workerInfo,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(workerInfo.CustomLabel))
            throw new ArgumentException("CUSTOM_LABEL_REQUIRED");

        workerInfo.CustomLabel = workerInfo.CustomLabel.Trim();

        var type = workerInfo.Type.Trim().ToLowerInvariant();

        if (type != "text" && type != "file")
            throw new ArgumentException("INVALID_WORKER_INFO_TYPE");

        workerInfo.Type = type;

        var duplicateExists = await _context.WorkerInfos
            .AnyAsync(
                w => w.WorkerId == workerId &&
                     w.CustomLabel != null &&
                     w.CustomLabel.ToLower() == workerInfo.CustomLabel.ToLower(),
                cancellationToken);

        if (duplicateExists)
            throw new InvalidOperationException("CUSTOM_LABEL_EXISTS");

        workerInfo.FieldId = null;
        workerInfo.UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Fill or update a preset field.
    /// If the worker_info row does not exist yet, create it using fields.allowed_type.
    /// </summary>
    private async Task<WorkerInfo> UpsertPresetFieldAsync(
        Guid workerId,
        WorkerInfo input,
        CancellationToken cancellationToken)
    {
        var field = await _context.Fields
            .Include(f => f.Category)
            .FirstOrDefaultAsync(f => f.Id == input.FieldId, cancellationToken)
            ?? throw new KeyNotFoundException("FIELD_NOT_FOUND");

        var existing = await _context.WorkerInfos
            .Include(w => w.Field)
                .ThenInclude(f => f!.Category)
            .FirstOrDefaultAsync(
                w => w.WorkerId == workerId && w.FieldId == input.FieldId,
                cancellationToken);

        if (existing != null)
        {
            existing.Value = input.Value;
            existing.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);
            return existing;
        }

        var created = new WorkerInfo
        {
            WorkerId = workerId,
            FieldId = field.Id,
            Field = field,
            CustomLabel = null,
            Type = field.AllowedType,
            Value = input.Value,
            UpdatedAt = DateTime.UtcNow
        };

        _context.WorkerInfos.Add(created);
        await _context.SaveChangesAsync(cancellationToken);

        return created;
    }

    /// <summary>
    /// Update a custom field.
    /// Type is not updated because it is immutable after creation.
    /// </summary>
    private async Task<WorkerInfo> UpdateCustomFieldAsync(
        Guid workerId,
        WorkerInfo input,
        CancellationToken cancellationToken)
    {
        var existing = await _context.WorkerInfos
            .Include(w => w.Field)
                .ThenInclude(f => f!.Category)
            .FirstOrDefaultAsync(
                w => w.WorkerId == workerId &&
                     w.Id == input.Id &&
                     w.FieldId == null,
                cancellationToken)
            ?? throw new KeyNotFoundException();

        if (input.CustomLabel != null)
        {
            if (string.IsNullOrWhiteSpace(input.CustomLabel))
                throw new ArgumentException("CUSTOM_LABEL_REQUIRED");

            var newLabel = input.CustomLabel.Trim();

            var duplicateExists = await _context.WorkerInfos
                .AnyAsync(
                    w => w.WorkerId == workerId &&
                         w.Id != existing.Id &&
                         w.CustomLabel != null &&
                         w.CustomLabel.ToLower() == newLabel.ToLower(),
                    cancellationToken);

            if (duplicateExists)
                throw new InvalidOperationException("CUSTOM_LABEL_EXISTS");

            existing.CustomLabel = newLabel;
        }

        existing.Value = input.Value;
        existing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return existing;
    }
}
