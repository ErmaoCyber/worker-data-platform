using Microsoft.EntityFrameworkCore;
using wdb_backend.Abstractions;
using wdb_backend.Common;
using wdb_backend.Data;
using wdb_backend.DTOs;
using wdb_backend.Models;

namespace wdb_backend.Services;

/// <summary>
/// Worker-side request review service.
/// Supports:
/// - list pending requests
/// - approve/reject requested fields
/// - approve/reject custom request by adding a new worker_info item
///
/// This version also creates a REQUEST_REVIEWED notification for the employer.
/// Blockchain side effects are intentionally not included here yet.
/// </summary>
public class WorkerRequestReviewServiceImpl : IWorkerRequestReviewService
{
    private readonly AppDbContext _context;

    public WorkerRequestReviewServiceImpl(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<WorkerActiveRequestDto>> GetActiveRequestsAsync(
        Guid workerId,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        var requests = await _context.Requests
            .Where(r =>
                r.WorkerId == workerId &&
                r.ExpiryDate > now &&
                (
                    r.Permissions.Any(p => p.Status == PermissionStatus.Pending) ||
                    r.CustomRequestStatus == "pending"
                ))
            .Include(r => r.Permissions)
                .ThenInclude(p => p.Field)
                    .ThenInclude(f => f!.Category)
            .Include(r => r.Permissions)
                .ThenInclude(p => p.WorkerInfo)
                    .ThenInclude(wi => wi!.Field)
                        .ThenInclude(f => f!.Category)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);

        var workerInfos = await _context.WorkerInfos
            .Where(w => w.WorkerId == workerId)
            .Include(w => w.Field)
                .ThenInclude(f => f!.Category)
            .ToListAsync(cancellationToken);

        var workerInfoByFieldId = workerInfos
            .Where(w => w.FieldId.HasValue)
            .ToDictionary(w => w.FieldId!.Value, w => w);

        var result = new List<WorkerActiveRequestDto>();

        foreach (var request in requests)
        {
            var employer = await _context.Employers
                .FirstOrDefaultAsync(e => e.Id == request.EmployerId, cancellationToken);

            var dto = new WorkerActiveRequestDto
            {
                RequestId = request.Id,
                EmployerId = request.EmployerId,
                CompanyName = employer?.Name ?? "Unknown",
                Reason = request.Reason,
                ExpiryDate = request.ExpiryDate,
                CreatedAt = request.CreatedAt
            };

            if (!string.IsNullOrWhiteSpace(request.CustomRequest) &&
                request.CustomRequestStatus == "pending")
            {
                dto.CustomRequest = new WorkerCustomRequestDto
                {
                    Description = request.CustomRequest,
                    Status = request.CustomRequestStatus
                };
            }

            var pendingPermissions = request.Permissions
                .Where(p => p.Status == PermissionStatus.Pending)
                .OrderBy(p => ResolveCategory(p, workerInfoByFieldId))
                .ThenBy(p => ResolveLabel(p, workerInfoByFieldId))
                .ToList();

            foreach (var permission in pendingPermissions)
            {
                var info = ResolveWorkerInfoForDisplay(permission, workerInfoByFieldId);

                var label = ResolveLabel(permission, workerInfoByFieldId);
                var category = ResolveCategory(permission, workerInfoByFieldId);
                var type = ResolveType(permission, workerInfoByFieldId);

                var hasValue = !string.IsNullOrWhiteSpace(info?.Value);
                var canApprove = info != null && hasValue;

                dto.Items.Add(new WorkerRequestReviewItemDto
                {
                    PermissionId = permission.Id,
                    FieldId = permission.FieldId ?? info?.FieldId,
                    InfoId = permission.InfoId ?? info?.Id,
                    Label = label,
                    Category = category,
                    Type = type,
                    Value = info?.Value,
                    Status = permission.Status,
                    HasValue = hasValue,
                    CanApprove = canApprove,
                    CannotApproveReason = canApprove
                        ? null
                        : "This field has no saved value. Please fill it in before approving."
                });
            }

            result.Add(dto);
        }

        return result;
    }

    public async Task SubmitReviewAsync(
        Guid workerId,
        Guid requestId,
        SubmitWorkerRequestReviewRequest request,
        CancellationToken cancellationToken = default)
    {
        var hasItemReviews = request.Items.Count > 0;
        var hasCustomDecision = request.CustomRequestDecision != null;

        if (!hasItemReviews && !hasCustomDecision)
            throw new InvalidOperationException("NO_REVIEW_ACTIONS");

        var dataRequest = await _context.Requests
            .Include(r => r.Permissions)
            .FirstOrDefaultAsync(
                r => r.Id == requestId && r.WorkerId == workerId,
                cancellationToken)
            ?? throw new KeyNotFoundException("REQUEST_NOT_FOUND");

        if (dataRequest.ExpiryDate <= DateTime.UtcNow)
            throw new InvalidOperationException("REQUEST_EXPIRED");

        await ReviewPermissionItemsAsync(
            workerId,
            dataRequest,
            request.Items,
            cancellationToken);

        if (request.CustomRequestDecision != null)
        {
            await ReviewCustomRequestAsync(
                workerId,
                dataRequest,
                request.CustomRequestDecision,
                cancellationToken);
        }

        // Notify the employer that the worker has reviewed the request.
        _context.Notifications.Add(new wdb_backend.Models.Notification
        {
            RecipientWorkerId = null,
            RecipientEmployerId = dataRequest.EmployerId,
            Type = "REQUEST_REVIEWED",
            RequestId = dataRequest.Id,
            IsRead = false
        });

        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task ReviewPermissionItemsAsync(
        Guid workerId,
        Request dataRequest,
        List<SubmitWorkerRequestReviewItem> items,
        CancellationToken cancellationToken)
    {
        var duplicatedPermissionIds = items
            .GroupBy(i => i.PermissionId)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        if (duplicatedPermissionIds.Any())
            throw new InvalidOperationException("DUPLICATE_REVIEW_ITEM");

        foreach (var item in items)
        {
            var permission = dataRequest.Permissions
                .FirstOrDefault(p => p.Id == item.PermissionId)
                ?? throw new KeyNotFoundException("PERMISSION_NOT_FOUND");

            if (permission.WorkerId != workerId)
                throw new UnauthorizedAccessException("PERMISSION_NOT_OWNED_BY_WORKER");

            if (permission.Status != PermissionStatus.Pending)
                throw new InvalidOperationException("PERMISSION_NOT_PENDING");

            var decision = item.Decision.Trim().ToLowerInvariant();

            if (decision == "approved")
            {
                var workerInfo = await ResolveWorkerInfoForApprovalAsync(
                    workerId,
                    permission,
                    cancellationToken);

                if (workerInfo == null || string.IsNullOrWhiteSpace(workerInfo.Value))
                    throw new InvalidOperationException("FIELD_VALUE_REQUIRED");

                permission.InfoId = workerInfo.Id;
                permission.Status = PermissionStatus.Approved;
                permission.LastUpdatedAt = DateTime.UtcNow;
            }
            else if (decision == "rejected")
            {
                permission.Status = PermissionStatus.Rejected;
                permission.LastUpdatedAt = DateTime.UtcNow;
            }
            else
            {
                throw new InvalidOperationException("INVALID_DECISION");
            }
        }
    }

    private async Task ReviewCustomRequestAsync(
        Guid workerId,
        Request dataRequest,
        SubmitWorkerCustomRequestDecision decision,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(dataRequest.CustomRequest) ||
            dataRequest.CustomRequestStatus != "pending")
        {
            throw new InvalidOperationException("CUSTOM_REQUEST_NOT_PENDING");
        }

        var normalizedDecision = decision.Decision.Trim().ToLowerInvariant();

        if (normalizedDecision == "rejected")
        {
            dataRequest.CustomRequestStatus = "rejected";
            return;
        }

        if (normalizedDecision != "approved")
            throw new InvalidOperationException("INVALID_CUSTOM_REQUEST_DECISION");

        var label = decision.Label?.Trim();
        var type = decision.Type?.Trim().ToLowerInvariant();
        var value = decision.Value?.Trim();

        if (string.IsNullOrWhiteSpace(label))
            throw new InvalidOperationException("CUSTOM_LABEL_REQUIRED");

        if (type != "text" && type != "file")
            throw new InvalidOperationException("INVALID_WORKER_INFO_TYPE");

        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidOperationException("CUSTOM_VALUE_REQUIRED");

        var duplicateExists = await _context.WorkerInfos
            .AnyAsync(
                w => w.WorkerId == workerId &&
                     w.CustomLabel != null &&
                     w.CustomLabel.ToLower() == label.ToLower(),
                cancellationToken);

        if (duplicateExists)
            throw new InvalidOperationException("CUSTOM_LABEL_EXISTS");

        var workerInfo = new WorkerInfo
        {
            WorkerId = workerId,
            FieldId = null,
            CustomLabel = label,
            Type = type,
            Value = value,
            UpdatedAt = DateTime.UtcNow
        };

        _context.WorkerInfos.Add(workerInfo);
        await _context.SaveChangesAsync(cancellationToken);

        var permission = new Permission
        {
            RequestId = dataRequest.Id,
            WorkerId = workerId,
            FieldId = null,
            InfoId = workerInfo.Id,
            Status = PermissionStatus.Approved,
            LastUpdatedAt = DateTime.UtcNow
        };

        _context.Permissions.Add(permission);
        dataRequest.CustomRequestStatus = "approved";
    }

    private static WorkerInfo? ResolveWorkerInfoForDisplay(
        Permission permission,
        Dictionary<Guid, WorkerInfo> workerInfoByFieldId)
    {
        if (permission.WorkerInfo != null)
            return permission.WorkerInfo;

        if (permission.FieldId.HasValue &&
            workerInfoByFieldId.TryGetValue(permission.FieldId.Value, out var presetInfo))
        {
            return presetInfo;
        }

        return null;
    }

    private static string ResolveLabel(
        Permission permission,
        Dictionary<Guid, WorkerInfo> workerInfoByFieldId)
    {
        var info = ResolveWorkerInfoForDisplay(permission, workerInfoByFieldId);

        return permission.Field?.Label
            ?? info?.Field?.Label
            ?? info?.CustomLabel
            ?? "Unknown";
    }

    private static string ResolveCategory(
        Permission permission,
        Dictionary<Guid, WorkerInfo> workerInfoByFieldId)
    {
        var info = ResolveWorkerInfoForDisplay(permission, workerInfoByFieldId);

        return permission.Field?.Category?.CategoryName
            ?? info?.Field?.Category?.CategoryName
            ?? "OtherInformation";
    }

    private static string ResolveType(
        Permission permission,
        Dictionary<Guid, WorkerInfo> workerInfoByFieldId)
    {
        var info = ResolveWorkerInfoForDisplay(permission, workerInfoByFieldId);

        return permission.Field?.AllowedType
            ?? info?.Type
            ?? "text";
    }

    private async Task<WorkerInfo?> ResolveWorkerInfoForApprovalAsync(
        Guid workerId,
        Permission permission,
        CancellationToken cancellationToken)
    {
        if (permission.InfoId.HasValue)
        {
            return await _context.WorkerInfos
                .FirstOrDefaultAsync(
                    w => w.Id == permission.InfoId.Value && w.WorkerId == workerId,
                    cancellationToken);
        }

        if (permission.FieldId.HasValue)
        {
            return await _context.WorkerInfos
                .FirstOrDefaultAsync(
                    w => w.WorkerId == workerId && w.FieldId == permission.FieldId.Value,
                    cancellationToken);
        }

        return null;
    }
}
