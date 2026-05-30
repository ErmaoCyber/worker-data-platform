using Microsoft.EntityFrameworkCore;
using wdb_backend.Abstractions;
using wdb_backend.Common;
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

        if (worker == null) return null;

        // Each row = one permission item. ExpiryDate is on Request.
        var latestRequests = await (
            from request in _dbContext.Requests.AsNoTracking()
            join employer in _dbContext.Employers.AsNoTracking() on request.EmployerId equals employer.Id
            join permission in _dbContext.Permissions.AsNoTracking() on request.Id equals permission.RequestId
            join workerInfo in _dbContext.WorkerInfos.AsNoTracking() on permission.InfoId equals workerInfo.Id into wiGroup
            from workerInfo in wiGroup.DefaultIfEmpty()
            where request.WorkerId == workerId
            orderby request.CreatedAt descending
            select new WorkerDashboardRequestDto
            {
                RequestId = request.Id,
                EmployerId = request.EmployerId,
                EmployerName = employer.Name,
                // Label: custom field label or preset field label
                RequestedInformation = workerInfo == null
                                        ? "Pending"
                                        : (workerInfo.CustomLabel ?? workerInfo.Field!.Label),
                CheckPurpose = request.Reason,
                CreatedAt = request.CreatedAt,
                Status = permission.Status,
                ExpiresAt = request.ExpiryDate   // moved from permission to request
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
                    cancellationToken);

                var employerAddresses = logs.Select(l => l.EmployerAddress).Distinct().ToList();
                var employers = await _dbContext.Employers
                    .AsNoTracking()
                    .Where(e => employerAddresses.Contains(e.BlockchainAddress))
                    .ToListAsync(cancellationToken);

                blockchainRecords = logs.Select(log =>
                {
                    var matchedEmployer = employers
                        .FirstOrDefault(e => e.BlockchainAddress == log.EmployerAddress);

                    return new BlockchainRecordDto
                    {
                        Action = log.Action,
                        ActionLabel = log.Action,
                        UserMessage = $"Your data was {log.Action.ToLower()} by {matchedEmployer?.Name ?? log.EmployerAddress}",
                        EmployerName = matchedEmployer?.Name ?? "Unknown",
                        EmployerAddress = log.EmployerAddress,
                        WorkerAddress = log.WorkerAddress,
                        TxHash = log.TxHash,
                        Date = log.Date
                    };
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Blockchain unavailable for worker {WorkerId}", workerId);
                blockchainAvailable = false;
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
}
