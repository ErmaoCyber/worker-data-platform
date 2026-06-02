namespace wdb_backend.DTOs;

/// <summary>
/// DTO returned by employer-facing endpoints that list requestable worker data fields.
/// For preset fields, Id is the fieldId.
/// For custom fields, Id is the workerInfoId.
/// </summary>
public class WorkerInfoDto
{
    /// <summary>
    /// Frontend selection id.
    /// Preset field: fields.id.
    /// Custom field: worker_info.id.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// worker_info.id.
    /// Null for preset fields that have not been saved by the worker yet.
    /// </summary>
    public Guid? InfoId { get; set; }

    /// <summary>
    /// fields.id for preset fields.
    /// Null for custom fields.
    /// </summary>
    public Guid? FieldId { get; set; }

    /// <summary>
    /// Display label.
    /// </summary>
    public required string Label { get; set; }

    /// <summary>
    /// Category name, e.g. PersonalInformation.
    /// Custom fields are shown as OtherInformation.
    /// </summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// "text" or "file".
    /// </summary>
    public string Type { get; set; } = "text";

    public string? Status { get; set; }

    public bool IsPreset { get; set; }

    public bool HasValue { get; set; }
}
