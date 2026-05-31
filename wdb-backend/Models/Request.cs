using System.ComponentModel.DataAnnotations.Schema;

namespace wdb_backend.Models;

/// <summary>
/// One request = one employer asking one worker for data access.
/// ExpiryDate applies to all permissions under this request.
/// CustomRequest is free text for fields the worker doesn't have yet.
/// Maps to the "request" table.
/// </summary>
[Table("request")]
public class Request
{
    [Column("id")]
    public Guid Id { get; set; }

    [Column("employer_id")]
    public Guid EmployerId { get; set; }

    [Column("worker_id")]
    public Guid WorkerId { get; set; }

    [Column("reason")]
    public required string Reason { get; set; }

    /// <summary>
    /// Unified expiry date for all permissions under this request. Required.
    /// </summary>
    [Column("expiry_date")]
    public DateTime ExpiryDate { get; set; }

    /// <summary>
    /// Free text describing an extra field the worker doesn't have yet.
    /// NULL if no custom request was made.
    /// </summary>
    [Column("custom_request")]
    public string? CustomRequest { get; set; }

    /// <summary>
    /// Tracks the worker's response to the custom request.
    /// NULL:       no custom request
    /// 'pending':  worker has not responded yet
    /// 'approved': worker created the item
    /// 'rejected': worker declined
    /// </summary>
    [Column("custom_request_status")]
    public string? CustomRequestStatus { get; set; }

    [Column("created_at")]
    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public DateTime CreatedAt { get; set; }

    // Navigation
    public Employer Employer { get; set; } = null!;
    public Worker Worker { get; set; } = null!;
    public ICollection<Permission> Permissions { get; set; } = new List<Permission>();
}
