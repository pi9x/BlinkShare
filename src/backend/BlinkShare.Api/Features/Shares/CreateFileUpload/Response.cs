namespace BlinkShare.Api.Features.Shares.CreateFileUpload;

public sealed record Response(
    Guid ShareId,
    string Code,
    string StorageKey,
    string UploadUrl,
    DateTimeOffset ExpiresAtUtc);
