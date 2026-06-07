namespace wdb_backend.Abstractions;

public interface ISupabaseStorageService
{
    // Creates a short-lived signed URL for a private object in the configured bucket.
    // objectPath is the path within the bucket (no leading slash, no bucket name).
    // Throws if the Supabase configuration is missing.
    Task<SignedUrlResult> CreateSignedUrlAsync(
        string objectPath,
        int expiresInSeconds = 900,
        CancellationToken cancellationToken = default);

    // Uploads a blob to the configured bucket at the given path.
    // objectPath is the path within the bucket (no leading slash, no bucket name).
    // Returns the stored objectPath on success; throws on Supabase error.
    Task<string> UploadAsync(
        Stream content,
        string objectPath,
        string contentType,
        CancellationToken cancellationToken = default);
}

public class SignedUrlResult
{
    public required string Url { get; set; }
    public required DateTime ExpiresAt { get; set; }
}
