using BlinkShare.Api.Infrastructure.ObjectStorage;

namespace BlinkShare.Api.UnitTests.TestDoubles;

internal sealed class FakeObjectStorage : IObjectStorage
{
    private readonly Func<ObjectStorageUploadRequest, ObjectStorageUploadResult> _createUploadTarget;
    private readonly Func<ObjectStorageDownloadRequest, ObjectStorageDownloadResult> _createDownloadTarget;
    private readonly HashSet<string> _deletedKeys = new(StringComparer.Ordinal);

    public FakeObjectStorage(
        Func<ObjectStorageUploadRequest, ObjectStorageUploadResult>? createUploadTarget = null,
        Func<ObjectStorageDownloadRequest, ObjectStorageDownloadResult>? createDownloadTarget = null)
    {
        _createUploadTarget = createUploadTarget ?? DefaultUploadTarget;
        _createDownloadTarget = createDownloadTarget ?? DefaultDownloadTarget;
    }

    public Task<ObjectStorageUploadResult> CreateUploadTargetAsync(
        ObjectStorageUploadRequest request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(_createUploadTarget(request));
    }

    public Task<ObjectStorageDownloadResult> CreateDownloadTargetAsync(
        ObjectStorageDownloadRequest request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(_createDownloadTarget(request));
    }

    public Task DeleteObjectAsync(string storageKey, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _deletedKeys.Add(storageKey);
        return Task.CompletedTask;
    }

    public bool WasDeleted(string storageKey) => _deletedKeys.Contains(storageKey);

    private static ObjectStorageUploadResult DefaultUploadTarget(ObjectStorageUploadRequest request) =>
        new(
            request.StorageKey,
            $"https://object-storage.test/upload/{Uri.EscapeDataString(request.StorageKey)}",
            "PUT");

    private static ObjectStorageDownloadResult DefaultDownloadTarget(ObjectStorageDownloadRequest request) =>
        new(
            request.StorageKey,
            $"https://object-storage.test/download/{Uri.EscapeDataString(request.StorageKey)}");
}
