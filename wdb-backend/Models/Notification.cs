using System.ComponentModel.DataAnnotations.Schema;

namespace wdb_backend.Models;

/// <summary>
/// System notifications sent to workers or employers.
/// Exactly one of RecipientWorkerId or RecipientEmployerId must be set.
///
/// Type values:
///   NEW_REQUEST       - sent to worker when employer creates a request
///   REQUEST_REVIEWED  - sent to employer when worker submits review
///   ACCESS_REVOKED    - sent to employer when worker revokes access
///
/// Maps to the "notification" table.
/// </summary>
[Table("notification")]
public class Notification
{
    [Column("id")]
    public Guid Id { get; set; }

    [Column("created_at")]
    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Set when this notification is for a worker. NULL otherwise.
    /// </summary>
    [Column("recipient_worker_id")]
    public Guid? RecipientWorkerId { get; set; }

    /// <summary>
    /// Set when this notification is for an employer. NULL otherwise.
    /// </summary>
    [Column("recipient_employer_id")]
    public Guid? RecipientEmployerId { get; set; }

    /// <summary>
    /// 'NEW_REQUEST' | 'REQUEST_REVIEWED' | 'ACCESS_REVOKED'
    /// </summary>
    [Column("type")]
    public required string Type { get; set; }

    [Column("request_id")]
    public Guid? RequestId { get; set; }

    [Column("is_read")]
    public bool IsRead { get; set; } = false;

    // Navigation
    public Worker? RecipientWorker { get; set; }
    public Employer? RecipientEmployer { get; set; }
    public Request? Request { get; set; }
}

/// <summary>
/// Notification type constants. The values are stored as text in the
/// notification.type column and must stay in sync with the CHECK
/// constraint configured in AppDbContext.
/// </summary>
public static class NotificationType
{
    public const string NewRequest      = "NEW_REQUEST";
    public const string DataAccessed    = "DATA_ACCESSED";
    public const string RequestReviewed = "REQUEST_REVIEWED";
    public const string AccessRevoked   = "ACCESS_REVOKED";
}
