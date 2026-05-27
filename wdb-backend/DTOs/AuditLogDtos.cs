namespace wdb_backend.Dtos;

/// <summary>
/// A single audit log record returned to the worker audit log page.
/// This DTO gives the frontend both user-friendly display fields and technical blockchain proof.
/// </summary>
public class AuditLogRecordDto
{
    /// <summary>
    /// Raw action value recorded on the blockchain.
    /// Example: PermissionRequested, PermissionApproved, PermissionRejected, DataViewed, PermissionRevoked.
    /// </summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>
    /// User-friendly action label for display.
    /// Example: Access Approved, Data Viewed, Access Revoked.
    /// </summary>
    public string ActionLabel { get; set; } = string.Empty;

    /// <summary>
    /// Short user-friendly explanation of what happened.
    /// </summary>
    public string UserMessage { get; set; } = string.Empty;

    /// <summary>
    /// Company or employer name matched from the database.
    /// If no matching employer is found, the frontend can fall back to the employer address.
    /// </summary>
    public string EmployerName { get; set; } = string.Empty;

    /// <summary>
    /// The employer blockchain address involved in this record.
    /// This is mainly used as technical proof or fallback display.
    /// </summary>
    public string EmployerAddress { get; set; } = string.Empty;

    /// <summary>
    /// The worker blockchain address involved in this record.
    /// This is mainly useful for technical checking.
    /// </summary>
    public string WorkerAddress { get; set; } = string.Empty;

    /// <summary>
    /// The blockchain transaction hash.
    /// This proves that the action was recorded on chain.
    /// </summary>
    public string TransactionHash { get; set; } = string.Empty;

    /// <summary>
    /// The block hash where this transaction was included.
    /// This is optional because the current blockchain response does not provide it yet.
    /// </summary>
    public string? BlockHash { get; set; }

    /// <summary>
    /// The date and time when the blockchain action happened.
    /// </summary>
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Response DTO for the worker audit log endpoint.
/// </summary>
public class WorkerAuditLogResponseDto
{
    /// <summary>
    /// The worker ID from the application database.
    /// </summary>
    public Guid WorkerId { get; set; }

    /// <summary>
    /// List of audit log records for this worker.
    /// </summary>
    public List<AuditLogRecordDto> Records { get; set; } = new();
}
