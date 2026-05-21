namespace wdb_backend.DTOs;

public class EmployerActiveAccessDto
{
    public required Guid RequestId { get; set; }

    public required Guid WorkerId { get; set; }

    public required string WorkerName { get; set; }

    public required string WorkerEmail { get; set; }

    public required string Reason { get; set; }

    public required DateTime GrantedAt { get; set; }

    public DateTime? ExpiryDate { get; set; }

    public required List<EmployerActiveAccessInfoDto> WorkerInfo { get; set; }
}

public class EmployerActiveAccessInfoDto
{
    public required Guid PermissionId { get; set; }

    public required string DataType { get; set; }

    public required string Value { get; set; }
}
