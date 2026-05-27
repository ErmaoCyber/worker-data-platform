namespace wdb_backend.DTOs;

public class WorkerInfoDto
{
    public Guid Id { get; set; }
    public required string Desc { get; set; }
    public string? Status { get; set; }
    public required string Category { get; set; }
}
