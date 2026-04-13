namespace BlinkShare.Api.Infrastructure.ObjectStorage;

public sealed record ObjectStorageDownloadResult(
    string StorageKey,
    string DownloadUrl);
