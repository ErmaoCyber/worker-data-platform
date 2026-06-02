using wdb_backend.Models;

namespace wdb_backend.Abstractions;

public interface IBlockchainService
{
    BlockchainKeyPair GenerateKeyPair();

    /// <summary>
    /// Legacy-compatible method.
    /// It writes a category-level record with generic fallback values.
    /// Prefer LogCategoryTransactionAsync for new business flows.
    /// </summary>
    Task<string> LogTransactionAsync(
        string privateKey,
        string employerAddress,
        string workerAddress,
        BlockchainAction action,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Writes a category-level data access event to blockchain.
    /// The event stores access metadata, not actual personal data values.
    /// </summary>
    Task<string> LogCategoryTransactionAsync(
        string privateKey,
        string employerAddress,
        string workerAddress,
        string requestId,
        string category,
        string permissionIds,
        string itemLabels,
        BlockchainAction action,
        CancellationToken cancellationToken = default);

    Task<List<BlockchainTransactionResponse>> GetWorkerLogsAsync(
        string workerAddress,
        CancellationToken cancellationToken = default);

    Task<List<BlockchainTransactionResponse>> GetEmployerLogsAsync(
        string employerAddress,
        CancellationToken cancellationToken = default);
}
