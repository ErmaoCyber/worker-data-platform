namespace wdb_backend.DTOs;

public class EmployerSentRequestDto
{
    public required Guid RequestId { get; set; }

    public required Guid WorkerId { get; set; }

    public required string WorkerName { get; set; }

    public required string WorkerEmail { get; set; }

    public required string Reason { get; set; }

    public required DateTime ExpiryDate { get; set; }

    public required DateTime CreatedAt { get; set; }

    public required DateTime LastUpdatedAt { get; set; }

    // Free-text custom request (null when not used).
    public string? CustomRequest { get; set; }

    // null / "pending" / "approved" / "rejected"
    public string? CustomRequestStatus { get; set; }

    public required List<EmployerSentRequestItemDto> Items { get; set; }
}

public class EmployerSentRequestItemDto
{
    public required Guid PermissionId { get; set; }

    public required string CategoryName { get; set; }

    public required string Label { get; set; }

    // 0 = Pending, 1 = Approved, 2 = Rejected, 3 = Revoked
    public required int Status { get; set; }

    // True when this row comes from a worker-created custom item
    // (i.e. permission points at a worker_info row with a custom_label).
    public required bool IsCustom { get; set; }
}
