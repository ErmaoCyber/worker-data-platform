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

                blockchainRecords = logs
                    .Take(5)
                    .Select(log => new BlockchainRecordDto
                    {
                        EmployerAddress = log.EmployerAddress,
                        WorkerAddress = log.WorkerAddress,
                        Action = log.Action,
                        TxHash = log.TxHash,
                        Date = log.Date
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
}
