using BlinkShare.Api.Infrastructure.ObjectStorage;

namespace BlinkShare.Worker.UnitTests.TestDoubles;

internal sealed class FakeObjectStorage : IObjectStorage
{
    private readonly HashSet<string> _deletedKeys = new(StringComparer.Ordinal);

    public Task<ObjectStorageUploadResult> CreateUploadTargetAsync(
        ObjectStorageUploadRequest request,
        CancellationToken cancellationToken) =>
        throw new NotSupportedException();

    public Task<ObjectStorageDownloadResult> CreateDownloadTargetAsync(
        ObjectStorageDownloadRequest request,
        CancellationToken cancellationToken) =>
        throw new NotSupportedException();

    public Task DeleteObjectAsync(string storageKey, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _deletedKeys.Add(storageKey);
        return Task.CompletedTask;
    }

    public bool WasDeleted(string storageKey) => _deletedKeys.Contains(storageKey);
}
