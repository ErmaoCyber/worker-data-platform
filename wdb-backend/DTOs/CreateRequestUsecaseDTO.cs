namespace wdb_backend.DTOs;

public class CreateRequestUsecaseDTO
{
    public required string Email { get; set; }

    /// <summary>
    /// Selected item IDs from the employer create request page.
    /// For preset fields, each value is a fieldId.
    /// For custom fields, each value is a workerInfoId.
    /// Kept as InfoDesc for frontend compatibility.
    /// </summary>
    public required List<string> InfoDesc { get; set; }

    public required string Reason { get; set; }
}
