using System.ComponentModel.DataAnnotations.Schema;

namespace wdb_backend.Models;

/// <summary>
/// One row per field per request.
///
/// Preset field request lifecycle:
///   Created:  FieldId SET, InfoId NULL, Status = Pending
///   Approved: InfoId SET (points to worker_info row), Status = Approved
///   Rejected: Status = Rejected, InfoId stays NULL
///
/// Custom field (via custom_request):
///   Created by worker responding to custom_request:
///   FieldId NULL, InfoId SET, Status = Approved (auto-approved)
///
/// Status values:
///   0 Pending  - awaiting worker decision
///   1 Approved - worker granted access
///   2 Rejected - worker denied (was never approved)
///   3 Revoked  - worker withdrew a previously approved access
///
/// Maps to the "permission" table.
/// </summary>
[Table("permission")]
public class Permission
{
    [Column("id")]
    public Guid Id { get; set; }

    [Column("request_id")]
    public Guid RequestId { get; set; }

    [Column("worker_id")]
    public Guid WorkerId { get; set; }

    /// <summary>
    /// Set for preset field requests. NULL for custom field permissions.
    /// </summary>
    [Column("field_id")]
    public Guid? FieldId { get; set; }

    /// <summary>
    /// Points to the actual worker_info row once approved.
    /// NULL while pending or rejected.
    /// Set to NULL by DB (ON DELETE SET NULL) if worker_info is deleted.
    /// </summary>
    [Column("info_id")]
    public Guid? InfoId { get; set; }

    /// <summary>
    /// 0 = Pending, 1 = Approved, 2 = Rejected, 3 = Revoked
    /// </summary>
    [Column("status")]
    public int Status { get; set; }

    [Column("last_updated_at")]
    public DateTime LastUpdatedAt { get; set; }

    // Navigation
    public Request Request { get; set; } = null!;
    public Field? Field { get; set; }
    public WorkerInfo? WorkerInfo { get; set; }
}

