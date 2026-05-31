using Microsoft.EntityFrameworkCore;
using wdb_backend.Abstractions;
using wdb_backend.Common;
using wdb_backend.Data;
using wdb_backend.DTOs;
using wdb_backend.Models;

namespace wdb_backend.Services;

public class WorkerDashboardServiceImpl : IWorkerDashboardService
{
    private const int PartiallyApprovedStatus = 4;
    private const int DashboardListLimit = 3;

    private readonly AppDbContext _dbContext;
    private readonly IBlockchainService _blockchainService;
    private readonly ILogger<WorkerDashboardServiceImpl> _logger;

    public WorkerDashboardServiceImpl(
        AppDbContext dbContext,
        IBlockchainService blockchainService,
        ILogger<WorkerDashboardServiceImpl> logger)
    {
        _dbContext = dbContext;
        _blockchainService = blockchainService;
        _logger = logger;
    }

    public async Task<WorkerDashboardResponseDto?> GetDashboardAsync(
        Guid workerId,
        CancellationToken cancellationToken = default)
    {
        var worker = await _dbContext.Workers
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == workerId, cancellationToken);

        if (worker == null)
        {
            return null;
        }

        var summary = await BuildSummaryAsync(workerId, cancellationToken);
        var latestRequests = await BuildLatestRequestsAsync(workerId, cancellationToken);
        var blockchainResult = await BuildBlockchainRecordsAsync(worker, workerId, cancellationToken);

        return new WorkerDashboardResponseDto
        {
            Worker = new WorkerBasicInfoDto
            {
                Id = worker.Id,
                Name = worker.Name,
                Email = worker.Email,
                Verified = worker.Verified,
                BlockchainAddress = worker.BlockchainAddress
            },
            Summary = summary,
            LatestRequests = latestRequests,
            BlockchainRecords = blockchainResult.Records,
            BlockchainAvailable = blockchainResult.Available
        };
    }

    private async Task<WorkerDashboardSummaryDto> BuildSummaryAsync(
        Guid workerId,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;

        var pendingReviews = await _dbContext.Requests
            .AsNoTracking()
            .Where(request => request.WorkerId == workerId)
            .Where(request =>
                request.Permissions.Any(permission => permission.Status == PermissionStatus.Pending) ||
                request.CustomRequestStatus == "pending")
            .CountAsync(cancellationToken);

        var activeAccess = await (
            from permission in _dbContext.Permissions.AsNoTracking()
            join request in _dbContext.Requests.AsNoTracking()
                on permission.RequestId equals request.Id
            where permission.WorkerId == workerId
                  && permission.Status == PermissionStatus.Approved
                  && request.ExpiryDate > now
            select permission.Id
        ).CountAsync(cancellationToken);

        var totalRequests = await _dbContext.Requests
            .AsNoTracking()
            .CountAsync(request => request.WorkerId == workerId, cancellationToken);

        return new WorkerDashboardSummaryDto
        {
            PendingReviews = pendingReviews,
            ActiveAccess = activeAccess,
            TotalRequests = totalRequests
        };
    }

    private async Task<List<WorkerDashboardRequestDto>> BuildLatestRequestsAsync(
        Guid workerId,
        CancellationToken cancellationToken)
    {
        var latestRequestRows = await (
            from request in _dbContext.Requests.AsNoTracking()
            join employer in _dbContext.Employers.AsNoTracking()
                on request.EmployerId equals employer.Id
            where request.WorkerId == workerId
            orderby request.CreatedAt descending
            select new
            {
                request.Id,
                request.EmployerId,
                EmployerName = employer.Name,
                request.Reason,
                request.CreatedAt,
                request.ExpiryDate,
                request.CustomRequest,
                request.CustomRequestStatus
            }
        )
        .Take(DashboardListLimit)
        .ToListAsync(cancellationToken);

        var requestIds = latestRequestRows
            .Select(request => request.Id)
            .ToList();

        if (requestIds.Count == 0)
        {
            return new List<WorkerDashboardRequestDto>();
        }

        var permissionRows = await _dbContext.Permissions
            .AsNoTracking()
            .Include(permission => permission.Field)
            .Include(permission => permission.WorkerInfo)
                .ThenInclude(workerInfo => workerInfo!.Field)
            .Where(permission => requestIds.Contains(permission.RequestId))
            .ToListAsync(cancellationToken);

        return latestRequestRows.Select(request =>
        {
            var permissionsForRequest = permissionRows
                .Where(permission => permission.RequestId == request.Id)
                .ToList();

            var requestedLabels = permissionsForRequest
                .Select(GetPermissionLabel)
                .Where(label => !string.IsNullOrWhiteSpace(label))
                .Distinct()
                .ToList();

            if (!string.IsNullOrWhiteSpace(request.CustomRequest))
            {
                requestedLabels.Add($"Custom request: {request.CustomRequest}");
            }

            return new WorkerDashboardRequestDto
            {
                RequestId = request.Id,
                EmployerId = request.EmployerId,
                EmployerName = request.EmployerName,
                RequestedInformation = requestedLabels.Count == 0
                    ? "No information listed"
                    : string.Join(", ", requestedLabels),
                CheckPurpose = request.Reason,
                CreatedAt = request.CreatedAt,
                Status = GetRequestDisplayStatus(
                    permissionsForRequest.Select(permission => permission.Status).ToList(),
                    request.CustomRequestStatus
                ),
                ExpiresAt = request.ExpiryDate
            };
        }).ToList();
    }

    private async Task<(List<BlockchainRecordDto> Records, bool Available)> BuildBlockchainRecordsAsync(
        Worker worker,
        Guid workerId,
        CancellationToken cancellationToken)
    {
        var blockchainRecords = new List<BlockchainRecordDto>();

        if (string.IsNullOrWhiteSpace(worker.BlockchainAddress))
        {
            return (blockchainRecords, true);
        }

        try
        {
            var logs = await _blockchainService.GetWorkerLogsAsync(
                worker.BlockchainAddress,
                cancellationToken);

            var recentLogs = logs
                .OrderByDescending(log => log.Date)
                .Take(DashboardListLimit)
                .ToList();

            var employerAddresses = recentLogs
                .Select(log => log.EmployerAddress)
                .Where(address => !string.IsNullOrWhiteSpace(address))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var employers = await _dbContext.Employers
                .AsNoTracking()
                .Where(employer =>
                    employer.BlockchainAddress != null &&
                    employerAddresses.Contains(employer.BlockchainAddress))
                .ToListAsync(cancellationToken);

            blockchainRecords = recentLogs.Select(log =>
            {
                var matchedEmployer = employers.FirstOrDefault(employer =>
                    employer.BlockchainAddress != null &&
                    employer.BlockchainAddress.Equals(
                        log.EmployerAddress,
                        StringComparison.OrdinalIgnoreCase));

                var employerName = matchedEmployer?.Name ?? "Unknown company";

                return new BlockchainRecordDto
                {
                    Action = log.Action,
                    ActionLabel = GetBlockchainActionLabel(log.Action),
                    UserMessage = GetBlockchainUserMessage(log, employerName),
                    EmployerName = employerName,
                    EmployerAddress = log.EmployerAddress,
                    WorkerAddress = log.WorkerAddress,
                    TxHash = log.TxHash,
                    Date = log.Date
                };
            }).ToList();

            return (blockchainRecords, true);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Blockchain unavailable for worker {WorkerId}", workerId);
            return (blockchainRecords, false);
        }
    }

    private static string GetPermissionLabel(Permission permission)
    {
        if (!string.IsNullOrWhiteSpace(permission.WorkerInfo?.CustomLabel))
        {
            return permission.WorkerInfo.CustomLabel;
        }

        if (!string.IsNullOrWhiteSpace(permission.WorkerInfo?.Field?.Label))
        {
            return permission.WorkerInfo.Field.Label;
        }

        if (!string.IsNullOrWhiteSpace(permission.Field?.Label))
        {
            return permission.Field.Label;
        }

        return "Unknown information";
    }

    private static int GetRequestDisplayStatus(
        List<int> permissionStatuses,
        string? customRequestStatus)
    {
        var normalizedCustomStatus = customRequestStatus?
            .Trim()
            .ToLowerInvariant();

        var hasPendingCustom = normalizedCustomStatus == "pending";
        var hasApprovedCustom = normalizedCustomStatus == "approved";
        var hasRejectedCustom = normalizedCustomStatus == "rejected";

        if (permissionStatuses.Count == 0)
        {
            if (hasPendingCustom) return PermissionStatus.Pending;
            if (hasApprovedCustom) return PermissionStatus.Approved;
            if (hasRejectedCustom) return PermissionStatus.Rejected;

            return PermissionStatus.Pending;
        }

        if (permissionStatuses.Any(status => status == PermissionStatus.Revoked))
        {
            return PermissionStatus.Revoked;
        }

        var hasPending =
            permissionStatuses.Any(status => status == PermissionStatus.Pending) ||
            hasPendingCustom;

        var hasApproved =
            permissionStatuses.Any(status => status == PermissionStatus.Approved) ||
            hasApprovedCustom;

        var hasRejected =
            permissionStatuses.Any(status => status == PermissionStatus.Rejected) ||
            hasRejectedCustom;

        if (hasPending && !hasApproved && !hasRejected)
        {
            return PermissionStatus.Pending;
        }

        if (hasApproved && !hasPending && !hasRejected)
        {
            return PermissionStatus.Approved;
        }

        if (hasRejected && !hasPending && !hasApproved)
        {
            return PermissionStatus.Rejected;
        }

        return PartiallyApprovedStatus;
    }

    private static string GetBlockchainActionLabel(string action)
    {
        return action switch
        {
            nameof(BlockchainAction.PermissionRequested) => "Request created",
            nameof(BlockchainAction.PermissionApproved) => "Access approved",
            nameof(BlockchainAction.PermissionRejected) => "Request rejected",
            nameof(BlockchainAction.DataViewed) => "Data viewed",
            nameof(BlockchainAction.PermissionRevoked) => "Access revoked",
            _ => "Blockchain activity"
        };
    }

    private static string GetBlockchainUserMessage(
        BlockchainTransactionResponse log,
        string employerName)
    {
        var itemText = string.IsNullOrWhiteSpace(log.ItemLabels)
            ? "your data"
            : log.ItemLabels;

        return log.Action switch
        {
            nameof(BlockchainAction.PermissionRequested) =>
                $"{employerName} requested access to {itemText}.",

            nameof(BlockchainAction.PermissionApproved) =>
                $"You approved access to {itemText} for {employerName}.",

            nameof(BlockchainAction.PermissionRejected) =>
                $"You rejected a data request from {employerName}.",

            nameof(BlockchainAction.DataViewed) =>
                $"{employerName} viewed approved data: {itemText}.",

            nameof(BlockchainAction.PermissionRevoked) =>
                $"You revoked {employerName}'s access to {itemText}.",

            _ =>
                $"A blockchain activity was recorded for {employerName}."
        };
    }
}
