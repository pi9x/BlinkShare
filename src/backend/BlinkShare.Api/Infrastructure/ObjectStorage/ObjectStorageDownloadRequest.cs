namespace BlinkShare.Api.Infrastructure.ObjectStorage;

public sealed record ObjectStorageDownloadRequest(
    string StorageKey,
    string? FileName,
    string? ContentType,
    DateTimeOffset ExpiresAtUtc);
