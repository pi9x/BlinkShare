namespace BlinkShare.Api.Features.AnonymousSessions.PublishFileMetadata;

public sealed record Command(
    Guid SessionId,
    Guid PeerId,
    string? ResumeToken,
    string? FileName,
    string? ContentType,
    long SizeBytes,
    string? ShareCode);
