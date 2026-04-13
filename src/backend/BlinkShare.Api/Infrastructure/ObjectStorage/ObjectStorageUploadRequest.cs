namespace BlinkShare.Api.Infrastructure.ObjectStorage;

public sealed record ObjectStorageUploadRequest(
    string StorageKey,
    string ContentType,
    long SizeBytes,
    DateTimeOffset ExpiresAtUtc);
