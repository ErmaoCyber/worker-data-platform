namespace wdb_backend.DTOs;

public class WorkerDashboardResponseDto
{
    public WorkerBasicInfoDto Worker { get; set; } = new();
    public List<WorkerDashboardRequestDto> LatestRequests { get; set; } = new();
    public List<BlockchainRecordDto> BlockchainRecords { get; set; } = new();
    public bool BlockchainAvailable { get; set; }
}

public class WorkerBasicInfoDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool Verified { get; set; }
    public string? BlockchainAddress { get; set; }
}

public class WorkerDashboardRequestDto
{
    public Guid RequestId { get; set; }
    public Guid EmployerId { get; set; }
    public string EmployerName { get; set; } = string.Empty;

    public string RequestedInformation { get; set; } = string.Empty;
    public string CheckPurpose { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
    public int Status { get; set; }
    public DateTime? ExpiresAt { get; set; }
}

public class BlockchainRecordDto
{
    public string Action { get; set; } = string.Empty;

    // User-friendly action label for dashboard display.
    public string ActionLabel { get; set; } = string.Empty;

    // Short explanation for normal users.
    public string UserMessage { get; set; } = string.Empty;

    // Company name matched from the employer table.
    public string EmployerName { get; set; } = string.Empty;

    // Technical blockchain fields kept as proof, but not shown as the main content.
    public string EmployerAddress { get; set; } = string.Empty;
    public string WorkerAddress { get; set; } = string.Empty;
    public string TxHash { get; set; } = string.Empty;

    public DateTime Date { get; set; }
}
