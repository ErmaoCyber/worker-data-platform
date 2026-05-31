using System.ComponentModel.DataAnnotations.Schema;
using wdb_backend.Abstractions;

namespace wdb_backend.Models;

[Table("employer")]
public class Employer : IUser
{
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("email")]
    public string Email { get; set; } = string.Empty;

    [Column("password")]
    public string Password { get; set; } = string.Empty;

    [Column("verified")]
    public bool Verified { get; set; } = false;

    [Column("blockchain_address")]
    public string? BlockchainAddress { get; set; }

    [Column("private_key")]
    public string? PrivateKey { get; set; }
}
