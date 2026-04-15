using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;

namespace BlinkShare.Api.Infrastructure.ObjectStorage;

public sealed class S3CompatibleObjectStorage : IObjectStorage, IDisposable
{
    private readonly ObjectStorageOptions _options;
    private readonly AmazonS3Client _internalClient;
    private readonly AmazonS3Client _publicClient;

    public S3CompatibleObjectStorage(IOptions<ObjectStorageOptions> options)
    {
        _options = options.Value;
        _internalClient = CreateClient(_options.ServiceUrl);
        _publicClient = CreateClient(_options.PublicUrl);
    }

    public Task<ObjectStorageUploadResult> CreateUploadTargetAsync(
        ObjectStorageUploadRequest request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var url = _publicClient.GetPreSignedURL(new GetPreSignedUrlRequest
        {
            BucketName = _options.BucketName,
            Key = request.StorageKey,
            Verb = HttpVerb.PUT,
            Expires = request.ExpiresAtUtc.UtcDateTime,
            ContentType = request.ContentType
        });

        return Task.FromResult(new ObjectStorageUploadResult(
            request.StorageKey,
            url,
            "PUT"));
    }

    public Task<ObjectStorageDownloadResult> CreateDownloadTargetAsync(
        ObjectStorageDownloadRequest request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var preSignedRequest = new GetPreSignedUrlRequest
        {
            BucketName = _options.BucketName,
            Key = request.StorageKey,
            Verb = HttpVerb.GET,
            Expires = request.ExpiresAtUtc.UtcDateTime
        };

        if (!string.IsNullOrWhiteSpace(request.ContentType))
        {
            preSignedRequest.ResponseHeaderOverrides.ContentType = request.ContentType;
        }

        if (!string.IsNullOrWhiteSpace(request.FileName))
        {
            preSignedRequest.ResponseHeaderOverrides.ContentDisposition =
                $"attachment; filename=\"{request.FileName.Replace("\"", string.Empty, StringComparison.Ordinal)}\"";
        }

        var url = _publicClient.GetPreSignedURL(preSignedRequest);

        return Task.FromResult(new ObjectStorageDownloadResult(
            request.StorageKey,
            url));
    }

    public async Task DeleteObjectAsync(string storageKey, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        await _internalClient.DeleteObjectAsync(new DeleteObjectRequest
        {
            BucketName = _options.BucketName,
            Key = storageKey
        }, cancellationToken);
    }

    public void Dispose()
    {
        _internalClient.Dispose();
        _publicClient.Dispose();
    }

    private AmazonS3Client CreateClient(string serviceUrl)
    {
        var config = new AmazonS3Config
        {
            ServiceURL = serviceUrl,
            AuthenticationRegion = _options.Region,
            ForcePathStyle = _options.ForcePathStyle
        };

        return new AmazonS3Client(
            new BasicAWSCredentials(_options.AccessKey, _options.SecretKey),
            config);
    }
}
