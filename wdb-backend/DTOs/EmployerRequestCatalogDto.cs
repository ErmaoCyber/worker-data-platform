namespace wdb_backend.DTOs;

/// <summary>
/// Catalog of fields and items the employer can request from a target worker.
/// Returned by GET /api/employer-request/catalog.
/// </summary>
public class EmployerRequestCatalogDto
{
    public required EmployerRequestCatalogWorkerDto Worker { get; set; }
    public required List<EmployerRequestCatalogCategoryDto> Categories { get; set; }
}

public class EmployerRequestCatalogWorkerDto
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Email { get; set; }
}

public class EmployerRequestCatalogCategoryDto
{
    public Guid Id { get; set; }
    public required string Name { get; set; }

    // Preset fields belonging to this category (empty for OtherInformation).
    public required List<EmployerRequestCatalogPresetFieldDto> PresetFields { get; set; }

    // Worker's own custom items (only populated for OtherInformation).
    public required List<EmployerRequestCatalogCustomItemDto> CustomItems { get; set; }
}

public class EmployerRequestCatalogPresetFieldDto
{
    public Guid FieldId { get; set; }
    public required string Label { get; set; }
    public required string AllowedType { get; set; }
}

public class EmployerRequestCatalogCustomItemDto
{
    public Guid WorkerInfoId { get; set; }
    public required string Label { get; set; }
    public required string Type { get; set; }
}
