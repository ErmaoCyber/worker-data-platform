namespace wdb_backend.DTOs;

/// <summary>
/// Payload submitted by the employer to create a new data access request.
/// PresetFieldIds / CustomWorkerInfoIds / CustomRequest may any combination
/// be present but at least one must be non-empty.
/// </summary>
public class CreateEmployerRequestDto
{
    public required string WorkerEmail { get; set; }
    public required string Reason { get; set; }
    public DateTime ExpiryDate { get; set; }
    public List<Guid> PresetFieldIds { get; set; } = new();
    public List<Guid> CustomWorkerInfoIds { get; set; } = new();
    public string? CustomRequest { get; set; }
}

public class CreateEmployerRequestResultDto
{
    public Guid RequestId { get; set; }
}
