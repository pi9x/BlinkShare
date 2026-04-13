namespace BlinkShare.Api.Features.AnonymousSessions.PublishFileMetadata;

public sealed record Response(
    Guid SessionId,
    Guid PeerId,
    string FileName,
    string ContentType,
    long SizeBytes,
    DateTimeOffset PublishedAtUtc);
