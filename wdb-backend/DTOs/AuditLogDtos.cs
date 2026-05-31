namespace wdb_backend.Dtos;

public class AuditLogRecordDto
{
    public string Action { get; set; } = string.Empty;
    public string ActionLabel { get; set; } = string.Empty;
    public string UserMessage { get; set; } = string.Empty;

    public string EmployerName { get; set; } = string.Empty;

    public string RequestId { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string CategoryLabel { get; set; } = string.Empty;

    public string PermissionIds { get; set; } = string.Empty;
    public List<string> ItemLabels { get; set; } = new();

    public string EmployerAddress { get; set; } = string.Empty;
    public string WorkerAddress { get; set; } = string.Empty;
    public string TransactionHash { get; set; } = string.Empty;
    public string? BlockHash { get; set; }

    public DateTime CreatedAt { get; set; }
}

public class WorkerAuditLogResponseDto
{
    public Guid WorkerId { get; set; }
    public List<AuditLogRecordDto> Records { get; set; } = new();
}
