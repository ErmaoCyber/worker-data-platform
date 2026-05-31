using System.ComponentModel.DataAnnotations.Schema;

namespace wdb_backend.Models;

/// <summary>
/// Global preset field definitions. Read-only after seed.
/// allowed_type locks the data type for preset fields ('text' or 'file').
/// Maps to the "fields" table.
/// </summary>
[Table("fields")]
public class Field
{
    [Column("id")]
    public Guid Id { get; set; }

    /// <summary>
    /// System key name e.g. "full_name", "date_of_birth".
    /// </summary>
    [Column("field")]
    public required string FieldName { get; set; }

    [Column("category_id")]
    public Guid CategoryId { get; set; }

    /// <summary>
    /// Display name e.g. "Full Name", "Date of Birth".
    /// </summary>
    [Column("label")]
    public required string Label { get; set; }

    /// <summary>
    /// Locks the type for this preset field. Either 'text' or 'file'.
    /// </summary>
    [Column("allowed_type")]
    public required string AllowedType { get; set; }

    // Navigation
    public Category Category { get; set; } = null!;
}
