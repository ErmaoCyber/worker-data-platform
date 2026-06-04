using System.Net.Http.Json;
using System.Text.Json.Serialization;
using wdb_backend.Abstractions;

namespace wdb_backend.Services;

public class SupabaseStorageService : ISupabaseStorageService
{
    private readonly HttpClient _httpClient;
    private readonly string? _supabaseUrl;
    private readonly string? _serviceRoleKey;
    private readonly string _bucket;

    public SupabaseStorageService(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient;
        _supabaseUrl = config["Supabase:Url"];
        _serviceRoleKey = config["Supabase:ServiceRoleKey"];
        _bucket = config["Supabase:DocumentsBucket"] ?? "worker-documents";
    }

    public async Task<SignedUrlResult> CreateSignedUrlAsync(
        string objectPath,
        int expiresInSeconds = 900,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_supabaseUrl) || string.IsNullOrWhiteSpace(_serviceRoleKey))
        {
            throw new InvalidOperationException(
                "Supabase storage configuration is missing (Supabase:Url and Supabase:ServiceRoleKey).");
        }

        var url = $"{_supabaseUrl}/storage/v1/object/sign/{_bucket}/{objectPath}";
        using var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Headers.Add("Authorization", $"Bearer {_serviceRoleKey}");
        request.Content = JsonContent.Create(new { expiresIn = expiresInSeconds });

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<SignedUrlPayload>(cancellationToken)
                      ?? throw new InvalidOperationException("Supabase returned an empty response.");

        if (string.IsNullOrEmpty(payload.SignedUrl))
        {
            throw new InvalidOperationException("Supabase did not return a signed URL.");
        }

        return new SignedUrlResult
        {
            Url = $"{_supabaseUrl}{payload.SignedUrl}",
            ExpiresAt = DateTime.UtcNow.AddSeconds(expiresInSeconds)
        };
    }

    private record SignedUrlPayload
    {
        [JsonPropertyName("signedURL")]
        public string? SignedUrl { get; init; }
    }


    public async Task<string> UploadAsync(
        Stream content,
        string objectPath,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_supabaseUrl) || string.IsNullOrWhiteSpace(_serviceRoleKey))
        {
            throw new InvalidOperationException(
                "Supabase storage configuration is missing (Supabase:Url and Supabase:ServiceRoleKey).");
        }

        var url = $"{_supabaseUrl}/storage/v1/object/{_bucket}/{objectPath}";
        using var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Headers.Add("Authorization", $"Bearer {_serviceRoleKey}");

        using var streamContent = new StreamContent(content);
        streamContent.Headers.ContentType =
            new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
        request.Content = streamContent;

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        return objectPath;
    }
}
