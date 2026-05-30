namespace wdb_backend.DTOs;

// ── Response DTOs ─────────────────────────────────────────────

/// <summary>A single field item returned in the profile response.</summary>
public class WorkerProfileFieldDto
{
    public Guid InfoId { get; set; }       // worker_info.id (null if not filled yet)
    public Guid? FieldId { get; set; }       // preset field id; null for custom
    public string Label { get; set; } = string.Empty;  // display label
    public string Type { get; set; } = string.Empty;  // "text" or "file"
    public string? Value { get; set; }       // null = not filled
    public bool IsPreset { get; set; }       // true = preset, false = custom
    public bool HasValue => Value != null;
}

/// <summary>A category group containing its fields.</summary>
public class WorkerProfileCategoryDto
{
    public string Category { get; set; } = string.Empty;
    public List<WorkerProfileFieldDto> Fields { get; set; } = new();
}

// ── Request DTOs ──────────────────────────────────────────────

/// <summary>PUT /api/worker/profile/preset — update value of a preset field.</summary>
public class UpdatePresetFieldRequest
{
    public Guid FieldId { get; set; }   // the preset field definition id
    public string? Value { get; set; }   // new value (null to clear)
}

/// <summary>POST /api/worker/profile/custom — create a new custom (Other) field.</summary>
public class CreateCustomFieldRequest
{
    public required string Label { get; set; }   // custom label
    public required string Type { get; set; }   // "text" or "file"
    public string? Value { get; set; }
}

/// <summary>PUT /api/worker/profile/custom/{id} — update label and/or value of a custom field.</summary>
public class UpdateCustomFieldRequest
{
    public string? Label { get; set; }   // new label (null = keep existing)
    public string? Value { get; set; }   // new value (null = keep existing)
}
