namespace wdb_backend.DTOs;

public class ActiveAccessDto
{
    public required Guid RequestId { get; set; }

    public required string CompanyName { get; set; }

    public required DateTime GrantedAt { get; set; }

    public required string Reason { get; set; }

    public required List<ActiveAccessInfoDto> WorkerInfo { get; set; }
}

public class ActiveAccessInfoDto
{
    public required Guid PermissionId { get; set; }

    public required string DataType { get; set; }
}
