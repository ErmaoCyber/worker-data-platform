namespace wdb_backend.DTOs;

// ── Response DTOs ─────────────────────────────────────────────

/// <summary>
/// A single field item returned in the worker profile response.
/// It supports both preset fields and custom worker-created fields.
/// </summary>
public class WorkerProfileFieldDto
{
    /// <summary>
    /// worker_info.id.
    /// Null means this is a preset field that has not been filled/saved yet.
    /// </summary>
    public Guid? InfoId { get; set; }

    /// <summary>
    /// fields.id for preset fields. Null for custom fields.
    /// </summary>
    public Guid? FieldId { get; set; }

    /// <summary>
    /// Display label.
    /// For preset fields: fields.label.
    /// For custom fields: worker_info.custom_label.
    /// </summary>
    public string Label { get; set; } = string.Empty;

    /// <summary>
    /// "text" or "file".
    /// For preset fields, this comes from fields.allowed_type.
    /// For custom fields, this comes from worker_info.type.
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Stored value. Null means the worker has not filled it in or has cleared it.
    /// </summary>
    public string? Value { get; set; }

    /// <summary>
    /// True for preset fields; false for custom fields.
    /// </summary>
    public bool IsPreset { get; set; }

    /// <summary>
    /// Simple frontend helper.
    /// </summary>
    public bool HasValue => !string.IsNullOrWhiteSpace(Value);
}

/// <summary>
/// A category group containing profile fields.
/// </summary>
public class WorkerProfileCategoryDto
{
    public string Category { get; set; } = string.Empty;
    public List<WorkerProfileFieldDto> Fields { get; set; } = new();
}

// ── Request DTOs ──────────────────────────────────────────────

/// <summary>
/// PUT /api/worker/profile/preset
/// Fill in or update the value of a preset field.
/// Label and type are locked by the fields table.
/// </summary>
public class UpdatePresetFieldRequest
{
    public Guid FieldId { get; set; }

    /// <summary>
    /// New value. Null means clear the value.
    /// </summary>
    public string? Value { get; set; }
}

/// <summary>
/// POST /api/worker/profile/custom
/// Create a new custom OtherInformation field.
/// </summary>
public class CreateCustomFieldRequest
{
    public required string Label { get; set; }

    /// <summary>
    /// Must be "text" or "file".
    /// Type is immutable after creation.
    /// </summary>
    public required string Type { get; set; }

    public string? Value { get; set; }
}

/// <summary>
/// PUT /api/worker/profile/custom/{id}
/// Update a custom field.
/// Label can be omitted to keep the existing label.
/// Value is treated as a replacement value; null means clear the value.
/// </summary>
public class UpdateCustomFieldRequest
{
    public string? Label { get; set; }
    public string? Value { get; set; }
}
