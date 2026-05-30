namespace wdb_backend.DTOs;

/// <summary>
/// DTO returned by employer-facing endpoints that list worker info fields.
/// Label replaces the old Desc field; Category is derived from the Field relation.
/// </summary>
public class WorkerInfoDto
{
    public Guid Id { get; set; }

    /// <summary>Display label: Field.Label for preset fields, CustomLabel for custom fields.</summary>
    public required string Label { get; set; }

    /// <summary>Category name, e.g. "PersonalInformation". Empty string for custom fields.</summary>
    public string Category { get; set; } = string.Empty;

    public string? Status { get; set; }
}
