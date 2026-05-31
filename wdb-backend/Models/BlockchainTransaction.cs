namespace wdb_backend.Models;

public class BlockchainKeyPair
{
    public string PrivateKey { get; set; } = string.Empty;
    public string BlockchainAddress { get; set; } = string.Empty;
}

public class BlockchainTransactionResponse
{
    public string EmployerAddress { get; set; } = string.Empty;
    public string WorkerAddress { get; set; } = string.Empty;

    public string RequestId { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;

    // Comma-separated values stored on-chain.
    public string PermissionIds { get; set; } = string.Empty;
    public string ItemLabels { get; set; } = string.Empty;

    public DateTime Date { get; set; }
    public string Action { get; set; } = string.Empty;
    public string TxHash { get; set; } = string.Empty;
    public string? BlockHash { get; set; }
}

public enum BlockchainAction
{
    PermissionRequested = 0,
    PermissionApproved = 1,
    PermissionRejected = 2,
    DataViewed = 3,
    PermissionRevoked = 4
}
