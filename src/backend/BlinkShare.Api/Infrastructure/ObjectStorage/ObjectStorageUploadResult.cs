namespace BlinkShare.Api.Infrastructure.ObjectStorage;

public sealed record ObjectStorageUploadResult(
    string StorageKey,
    string UploadUrl,
    string HttpMethod);
