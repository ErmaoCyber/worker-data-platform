using Microsoft.EntityFrameworkCore;
using wdb_backend.Abstractions;
using wdb_backend.Data;
using wdb_backend.DTOs;

namespace wdb_backend.Services;

public class WorkerDashboardServiceImpl : IWorkerDashboardService
{
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

        // Each row represents one requested permission item.
        // This gives the frontend enough data to show a dashboard table.
        var latestRequests = await (
            from request in _dbContext.Requests.AsNoTracking()
            join employer in _dbContext.Employers.AsNoTracking()
                on request.EmployerId equals employer.Id
            join permission in _dbContext.Permissions.AsNoTracking()
                on request.Id equals permission.RequestId
            join workerInfo in _dbContext.WorkerInfos.AsNoTracking()
                on permission.InfoId equals workerInfo.Id
            where request.WorkerId == workerId
            orderby request.CreatedAt descending
            select new WorkerDashboardRequestDto
            {
                RequestId = request.Id,
                EmployerId = request.EmployerId,
                EmployerName = employer.Name,

                RequestedInformation = workerInfo.Desc,
                CheckPurpose = request.Reason,

                CreatedAt = request.CreatedAt,
                Status = (int)permission.Status,
                ExpiresAt = permission.ExpiryDate
            }
        )
        .Take(5)
        .ToListAsync(cancellationToken);

        var blockchainRecords = new List<BlockchainRecordDto>();
        var blockchainAvailable = true;

        if (!string.IsNullOrWhiteSpace(worker.BlockchainAddress))
        {
            try
            {
                var logs = await _blockchainService.GetWorkerLogsAsync(
                    worker.BlockchainAddress,
                    cancellationToken
                );

                // Match employer blockchain addresses from the blockchain logs with employer records in the database.
                // This lets the dashboard show a company name instead of only a technical address.
                var employerAddresses = logs
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

                var employerNameByAddress = employers
                    .Where(employer => !string.IsNullOrWhiteSpace(employer.BlockchainAddress))
                    .ToDictionary(
                        employer => employer.BlockchainAddress!,
                        employer => employer.Name,
                        StringComparer.OrdinalIgnoreCase);

                blockchainRecords = logs
                    .OrderByDescending(log => log.Date)
                    .Take(5)
                    .Select(log =>
                    {
                        var employerName = employerNameByAddress.TryGetValue(
                            log.EmployerAddress,
                            out var matchedEmployerName)
                            ? matchedEmployerName
                            : "Unknown company";

                        return new BlockchainRecordDto
                        {
                            Action = log.Action,
                            ActionLabel = GetActionLabel(log.Action),
                            UserMessage = GetUserMessage(log.Action, employerName),
                            EmployerName = employerName,
                            EmployerAddress = log.EmployerAddress,
                            WorkerAddress = log.WorkerAddress,
                            TxHash = log.TxHash,
                            Date = log.Date
                        };
                    })
                    .ToList();
            }
            catch (Exception ex)
            {
                blockchainAvailable = false;

                _logger.LogWarning(
                    ex,
                    "Failed to load blockchain records for worker {WorkerId}",
                    workerId
                );
            }
        }

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
            LatestRequests = latestRequests,
            BlockchainRecords = blockchainRecords,
            BlockchainAvailable = blockchainAvailable
        };
    }

    private static string GetActionLabel(string action)
    {
        return action switch
        {
            "PermissionRequested" => "Access Requested",
            "PermissionApproved" => "Access Approved",
            "PermissionRejected" => "Request Rejected",
            "DataViewed" => "Data Viewed",
            "PermissionRevoked" => "Access Revoked",
            _ => "Access Record"
        };
    }

    private static string GetUserMessage(string action, string employerName)
    {
        return action switch
        {
            "PermissionRequested" =>
                $"{employerName} requested access to your information.",

            "PermissionApproved" =>
                $"You approved {employerName} to access your information.",

            "PermissionRejected" =>
                $"You rejected {employerName}'s request to access your information.",

            "DataViewed" =>
                $"{employerName} viewed information you had approved.",

            "PermissionRevoked" =>
                $"You removed {employerName}'s access to your information.",

            _ =>
                $"An access-related action involving {employerName} was recorded."
        };
    }
}
