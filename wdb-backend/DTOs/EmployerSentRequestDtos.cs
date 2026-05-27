namespace wdb_backend.DTOs;

public class EmployerSentRequestDto
{
    public required Guid RequestId { get; set; }

    public required Guid WorkerId { get; set; }

    public required string WorkerName { get; set; }

    public required string WorkerEmail { get; set; }

    public required string Reason { get; set; }

    public required DateTime RequestedAt { get; set; }

    public required DateTime LastUpdatedAt { get; set; }

    public required string Status { get; set; }

    public required List<string> RequestedDataTypes { get; set; }
}
