using System.ComponentModel.DataAnnotations.Schema;

namespace wdb_backend.Models;

[Table("notification")]
public class Notification
{
    [Column("id")]
    public Guid Id { get; set; }

    [Column("employer_id")]
    public Guid EmployerId { get; set; }

    [Column("worker_id")]
    public Guid WorkerId { get; set; }

    [Column("worker_info_id")]
    public Guid WorkerInfoId { get; set; }

    [Column("type")]
    public string Type { get; set; }

    [Column("is_read")]
    public bool IsRead { get; set; }

    [Column("created_at")]
    public DateTime CreateAt { get; set; }
}
