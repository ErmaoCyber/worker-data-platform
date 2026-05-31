namespace wdb_backend.DTOs;

public class EmployerActiveAccessDto
{
    public required Guid RequestId { get; set; }

    public required Guid WorkerId { get; set; }

    public required string WorkerName { get; set; }

    public required string WorkerEmail { get; set; }

    public required string Reason { get; set; }

    public required DateTime GrantedAt { get; set; }

    public required DateTime ExpiryDate { get; set; }

    public required List<EmployerActiveAccessCategoryDto> Categories { get; set; }
}

public class EmployerActiveAccessCategoryDto
{
    public required string Name { get; set; }

    public required List<EmployerActiveAccessItemDto> Items { get; set; }
}

public class EmployerActiveAccessItemDto
{
    public required Guid PermissionId { get; set; }

    public required string Label { get; set; }

    // 'text' or 'file'
    public required string Type { get; set; }

    public required bool IsCustom { get; set; }
}
