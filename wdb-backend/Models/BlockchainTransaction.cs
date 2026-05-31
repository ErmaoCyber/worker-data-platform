namespace wdb_backend.Models;

/// <summary>
/// Represents a blockchain key pair for a worker or employer.
/// Stores the private key and the derived blockchain address.
/// </summary>
public class BlockchainKeyPair
{
    public string PrivateKey { get; set; } = string.Empty;
    public string BlockchainAddress { get; set; } = string.Empty;
}

/// <summary>
/// Represents a single transaction record returned from the blockchain.
/// Used by the audit log to display on-chain activity to workers and employers.
/// </summary>
public class BlockchainTransactionResponse
{
    public string EmployerAddress { get; set; } = string.Empty;
    public string WorkerAddress { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string Action { get; set; } = string.Empty;
    public string TxHash { get; set; } = string.Empty;
}

/// <summary>
/// The four on-chain action types recorded in this system.
///
/// RequestCreated  - Employer submits a data access request
/// RequestReviewed - Worker submits their review (contains per-item approve/reject results)
/// DataAccessed    - Employer views a specific approved field
/// AccessRevoked   - Worker revokes a previously approved access
/// </summary>
public enum BlockchainAction
{
    RequestCreated = 0,
    RequestReviewed = 1,
    DataAccessed = 2,
    AccessRevoked = 3
}
