using wdb_backend.Models;

namespace wdb_backend.Abstractions;

public interface IBlockchainAuditService
{
    // Best-effort on-chain log: looks up employer + worker blockchain credentials,
    // writes the action to the chain, and swallows any failure with a warning so
    // the business action is never blocked by chain unavailability.
    Task TryLogAsync(
        Guid employerId,
        Guid workerId,
        BlockchainAction action,
        CancellationToken ct = default);
}
