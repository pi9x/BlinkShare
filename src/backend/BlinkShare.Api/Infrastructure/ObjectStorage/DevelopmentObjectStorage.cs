using System.Collections.Concurrent;
using System.Web;
using Microsoft.Extensions.Options;

namespace BlinkShare.Api.Infrastructure.ObjectStorage;

public sealed class DevelopmentObjectStorage(IOptions<ObjectStorageOptions> options) : IObjectStorage
{
    private static readonly HttpClient HttpClient = new();
    private readonly ConcurrentDictionary<string, DateTimeOffset> _deletedKeys = new(StringComparer.Ordinal);

    public Task<ObjectStorageUploadResult> CreateUploadTargetAsync(
        ObjectStorageUploadRequest request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var uriBuilder = new UriBuilder(options.Value.BaseUrl)
        {
            Path = $"/upload/{Uri.EscapeDataString(request.StorageKey)}"
        };

        var query = HttpUtility.ParseQueryString(string.Empty);
        query["contentType"] = request.ContentType;
        query["sizeBytes"] = request.SizeBytes.ToString();
        query["expiresAtUtc"] = request.ExpiresAtUtc.ToString("O");
        uriBuilder.Query = query.ToString();

        return Task.FromResult(new ObjectStorageUploadResult(
            request.StorageKey,
            uriBuilder.Uri.ToString(),
            "PUT"));
    }

    public Task<ObjectStorageDownloadResult> CreateDownloadTargetAsync(
        ObjectStorageDownloadRequest request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var uriBuilder = new UriBuilder(options.Value.BaseUrl)
        {
            Path = $"/download/{Uri.EscapeDataString(request.StorageKey)}"
        };

        var query = HttpUtility.ParseQueryString(string.Empty);
        query["expiresAtUtc"] = request.ExpiresAtUtc.ToString("O");

        if (!string.IsNullOrWhiteSpace(request.FileName))
        {
            query["fileName"] = request.FileName;
        }

        if (!string.IsNullOrWhiteSpace(request.ContentType))
        {
            query["contentType"] = request.ContentType;
        }

        uriBuilder.Query = query.ToString();

        return Task.FromResult(new ObjectStorageDownloadResult(
            request.StorageKey,
            uriBuilder.Uri.ToString()));
    }

    public async Task DeleteObjectAsync(string storageKey, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        _deletedKeys[storageKey] = DateTimeOffset.UtcNow;

        var uriBuilder = new UriBuilder(options.Value.BaseUrl)
        {
            Path = $"/objects/{Uri.EscapeDataString(storageKey)}"
        };

        using var request = new HttpRequestMessage(HttpMethod.Delete, uriBuilder.Uri);

        try
        {
            using var response = await HttpClient.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();
        }
        catch (HttpRequestException)
        {
            // Development storage remains best-effort when no backing mock service is available.
        }
    }

    public bool WasDeleted(string storageKey) => _deletedKeys.ContainsKey(storageKey);
}
