namespace wdb_backend.Dtos;

/// <summary>
/// A single audit log record returned to the worker audit log page.
/// This DTO hides blockchain complexity and gives the frontend a clear structure to display.
/// </summary>
public class AuditLogRecordDto
{
    /// <summary>
    /// The action recorded on the blockchain.
    /// Example: PermissionRequested, PermissionApproved, PermissionRejected, DataViewed, PermissionRevoked.
    /// </summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>
    /// The employer address involved in this blockchain record.
    /// This may be shown in a shortened format on the frontend.
    /// </summary>
    public string EmployerAddress { get; set; } = string.Empty;

    /// <summary>
    /// The worker address involved in this blockchain record.
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
