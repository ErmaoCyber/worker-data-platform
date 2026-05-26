using wdb_backend.Models;
namespace wdb_backend.DTOs;



public class CreateRequestUsecaseDTO
{
    public required string Email { get; set; }
    public required List<string> InfoDesc { get; set; }
    public required string Reason { get; set; }
}

