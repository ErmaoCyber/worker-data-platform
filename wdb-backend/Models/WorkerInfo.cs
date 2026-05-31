using System.ComponentModel.DataAnnotations.Schema;

namespace wdb_backend.Models;

/// <summary>
/// Per-worker data store for both preset and custom fields.
///
/// Preset field:  FieldId NOT NULL, CustomLabel NULL
/// Custom field:  FieldId NULL,     CustomLabel NOT NULL
///
/// Type is locked at creation and must never be updated.
/// Value NULL means "not yet filled".
///
/// Maps to the "worker_info" table.
/// </summary>
[Table("worker_info")]
public class WorkerInfo
{
    [Column("id")]
    public Guid Id { get; set; }

    [Column("worker_id")]
    public Guid WorkerId { get; set; }

    /// <summary>
    /// Set for preset fields. NULL for custom fields.
    /// </summary>
    [Column("field_id")]
    public Guid? FieldId { get; set; }

    /// <summary>
    /// Set for custom fields (worker-defined label). NULL for preset fields.
    /// </summary>
    [Column("custom_label")]
    public string? CustomLabel { get; set; }

    /// <summary>
    /// 'text' or 'file'. Locked at creation - never update this column.
    /// For preset fields must equal Field.AllowedType.
    /// </summary>
    [Column("type")]
    public required string Type { get; set; }

    /// <summary>
    /// For type='text': the actual text value.
    /// For type='file': the Supabase Storage object path.
    /// NULL means the worker has not filled this field yet.
    /// </summary>
    [Column("value")]
    public string? Value { get; set; }

    [Column("created_at")]
    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; }

    // Navigation
    public Worker Worker { get; set; } = null!;
    public Field? Field { get; set; }
    public ICollection<Permission> Permissions { get; set; } = new List<Permission>();
}
