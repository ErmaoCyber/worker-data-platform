using Microsoft.EntityFrameworkCore;
using wdb_backend.Abstractions;
using wdb_backend.Data;
using wdb_backend.Models;

namespace wdb_backend.Services;

public class BlockchainAuditService : IBlockchainAuditService
{
    private readonly AppDbContext _context;
    private readonly IBlockchainService _blockchain;
    private readonly ILogger<BlockchainAuditService> _logger;

    public BlockchainAuditService(
        AppDbContext context,
        IBlockchainService blockchain,
        ILogger<BlockchainAuditService> logger)
    {
        _context = context;
        _blockchain = blockchain;
        _logger = logger;
    }

    public async Task TryLogAsync(
        Guid employerId,
        Guid workerId,
        BlockchainAction action,
        CancellationToken ct = default)
    {
        try
        {
            var employer = await _context.Employers
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == employerId, ct);
            var worker = await _context.Workers
                .AsNoTracking()
                .FirstOrDefaultAsync(w => w.Id == workerId, ct);

            if (employer == null || worker == null)
            {
                _logger.LogWarning(
                    "Skipping on-chain {Action}: employer or worker not found ({EmployerId} / {WorkerId})",
                    action, employerId, workerId);
                return;
            }

            if (string.IsNullOrWhiteSpace(employer.PrivateKey)
                || string.IsNullOrWhiteSpace(employer.BlockchainAddress)
                || string.IsNullOrWhiteSpace(worker.BlockchainAddress))
            {
                _logger.LogWarning(
                    "Skipping on-chain {Action}: missing blockchain credentials (employer={EmployerId}, worker={WorkerId})",
                    action, employerId, workerId);
                return;
            }

            var txHash = await _blockchain.LogTransactionAsync(
                employer.PrivateKey,
                employer.BlockchainAddress,
                worker.BlockchainAddress,
                action,
                ct);

            _logger.LogInformation(
                "On-chain {Action} logged, tx={TxHash}",
                action, txHash);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to log {Action} on-chain", action);
        }
    }
}
