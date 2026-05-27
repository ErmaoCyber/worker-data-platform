using Microsoft.EntityFrameworkCore;
using wdb_backend.Abstractions;
using wdb_backend.Data;
using wdb_backend.Dtos;

namespace wdb_backend.Services;

/// <summary>
/// Provides audit log data for the worker audit log page.
/// This service connects the application worker ID with the worker's blockchain address,
/// then reads blockchain logs and adds user-friendly context for the frontend.
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

        // Match employer blockchain addresses from the on-chain logs with employer records in the database.
        // This lets the frontend show a company name instead of only a blockchain address.
        var employerAddresses = blockchainRecords
            .Select(record => record.EmployerAddress)
            .Where(address => !string.IsNullOrWhiteSpace(address))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var employers = await _dbContext.Employers
            .AsNoTracking()
            .Where(employer =>
                employer.BlockchainAddress != null &&
                employerAddresses.Contains(employer.BlockchainAddress))
            .ToListAsync();

        var employerNameByAddress = employers
            .Where(employer => !string.IsNullOrWhiteSpace(employer.BlockchainAddress))
            .ToDictionary(
                employer => employer.BlockchainAddress!,
                employer => employer.Name,
                StringComparer.OrdinalIgnoreCase);

        var records = blockchainRecords
            .Select(record =>
            {
                var employerName = employerNameByAddress.TryGetValue(
                    record.EmployerAddress,
                    out var matchedEmployerName)
                    ? matchedEmployerName
                    : "Unknown company";

                return new AuditLogRecordDto
                {
                    Action = record.Action,
                    ActionLabel = GetActionLabel(record.Action),
                    UserMessage = GetUserMessage(record.Action, employerName),
                    EmployerName = employerName,
                    EmployerAddress = record.EmployerAddress,
                    WorkerAddress = record.WorkerAddress,
                    TransactionHash = record.TxHash,
                    BlockHash = null,
                    CreatedAt = record.Date
                };
            })
            .OrderByDescending(record => record.CreatedAt)
            .ToList();

        return new WorkerAuditLogResponseDto
        {
            WorkerId = workerId,
            Records = records
        };
    }

    /// <summary>
    /// Converts the raw blockchain action into a user-friendly label.
    /// </summary>
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

    /// <summary>
    /// Creates a short explanation for normal users.
    /// </summary>
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
