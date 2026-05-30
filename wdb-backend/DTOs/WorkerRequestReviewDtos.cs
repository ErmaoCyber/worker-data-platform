namespace wdb_backend.DTOs;

/// <summary>
/// A pending request shown on the worker data access review page.
/// </summary>
public class WorkerActiveRequestDto
{
    public Guid RequestId { get; set; }
    public Guid EmployerId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public DateTime ExpiryDate { get; set; }
    public DateTime CreatedAt { get; set; }

    public List<WorkerRequestReviewItemDto> Items { get; set; } = new();
}

/// <summary>
/// A single permission item inside a pending request.
/// </summary>
public class WorkerRequestReviewItemDto
{
    public Guid PermissionId { get; set; }

    /// <summary>
    /// Preset field id. Null for custom fields.
    /// </summary>
    public Guid? FieldId { get; set; }

    /// <summary>
    /// worker_info.id. May be null before approving a preset field.
    /// </summary>
    public Guid? InfoId { get; set; }

    public string Label { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string? Value { get; set; }

    public int Status { get; set; }
    public bool HasValue { get; set; }
    public bool CanApprove { get; set; }
    public string? CannotApproveReason { get; set; }
}

/// <summary>
/// POST /api/worker/data-access/requests/{requestId}/review
/// </summary>
public class SubmitWorkerRequestReviewRequest
{
    public List<SubmitWorkerRequestReviewItem> Items { get; set; } = new();
}

public class SubmitWorkerRequestReviewItem
{
    public Guid PermissionId { get; set; }

    /// <summary>
    /// "approved" or "rejected".
    /// </summary>
    public string Decision { get; set; } = string.Empty;
}
