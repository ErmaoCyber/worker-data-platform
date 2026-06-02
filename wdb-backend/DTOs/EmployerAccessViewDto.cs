namespace wdb_backend.DTOs;

public class EmployerAccessViewResultDto
{
    // 'text' or 'file'
    public required string Type { get; set; }

    // Filled when Type='text'.
    public string? Value { get; set; }

    // Filled when Type='file'. A short-lived signed URL.
    public string? Url { get; set; }

    public DateTime? UrlExpiresAt { get; set; }
}
