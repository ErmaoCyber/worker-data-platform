using System.ComponentModel.DataAnnotations.Schema;

namespace wdb_backend.Models;

/// <summary>
/// Global preset category dictionary. Read-only after seed.
/// Maps to the "categories" table.
/// </summary>
[Table("categories")]
public class Category
{
  [Column("id")]
  public Guid Id { get; set; }

  [Column("category")]
  public required string CategoryName { get; set; }

  // Navigation
  public ICollection<Field> Fields { get; set; } = new List<Field>();
}
