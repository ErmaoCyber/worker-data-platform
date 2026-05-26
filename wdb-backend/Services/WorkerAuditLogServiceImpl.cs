using Microsoft.EntityFrameworkCore;
using wdb_backend.Abstractions;
using wdb_backend.Data;
using wdb_backend.Dtos;

namespace wdb_backend.Services;

/// <summary>
/// Provides audit log data for the worker audit log page.
/// This service connects the application worker ID with the worker's blockchain address,
/// then reads the worker's blockchain logs.
/// </summary>
public class WorkerAuditLogServiceImpl : IWorkerAuditLogService
{
    private readonly AppDbContext _dbContext;
    private readonly IBlockchainService _blockchainService;

    public WorkerAuditLogServiceImpl(
        AppDbContext dbContext,
        IBlockchainService blockchainService)
    {
        _dbContext = dbContext;
        _blockchainService = blockchainService;
    }

    /// <summary>
    /// Gets audit log records for a worker.
    /// </summary>
    /// <param name="workerId">
    /// The worker ID from the application database.
    /// </param>
    /// <returns>
    /// A response containing the worker ID and blockchain audit records.
    /// </returns>
    public async Task<WorkerAuditLogResponseDto> GetWorkerAuditLogAsync(Guid workerId)
    {
        // Find the worker first because the blockchain service needs the worker's blockchain address,
        // not the database worker ID.
        var worker = await _dbContext.Workers
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.Id == workerId);

        if (worker == null)
        {
            throw new KeyNotFoundException("Worker not found.");
        }

        // If the worker does not have a blockchain address yet, there are no on-chain logs to show.
        if (string.IsNullOrWhiteSpace(worker.BlockchainAddress))
        {
            return new WorkerAuditLogResponseDto
            {
                WorkerId = workerId,
                Records = new List<AuditLogRecordDto>()
            };
        }

        var blockchainRecords = await _blockchainService.GetWorkerLogsAsync(worker.BlockchainAddress);

        var records = blockchainRecords
            .Select(record => new AuditLogRecordDto
            {
                Action = record.Action,
                EmployerAddress = record.EmployerAddress,
                WorkerAddress = record.WorkerAddress,
                TransactionHash = record.TxHash,
                BlockHash = null,
                CreatedAt = record.Date
            })
            .OrderByDescending(record => record.CreatedAt)
            .ToList();

        return new WorkerAuditLogResponseDto
        {
            WorkerId = workerId,
            Records = records
        };
    }
}
