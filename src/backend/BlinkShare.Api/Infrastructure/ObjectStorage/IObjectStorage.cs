namespace BlinkShare.Api.Infrastructure.ObjectStorage;

public interface IObjectStorage
{
    Task<ObjectStorageUploadResult> CreateUploadTargetAsync(
        ObjectStorageUploadRequest request,
        CancellationToken cancellationToken);

    Task<ObjectStorageDownloadResult> CreateDownloadTargetAsync(
        ObjectStorageDownloadRequest request,
        CancellationToken cancellationToken);

    Task DeleteObjectAsync(string storageKey, CancellationToken cancellationToken);
}
