namespace BlinkShare.Api.Features.AnonymousSessions;

public sealed record PeerConnectedMessage(
    Guid SessionId,
    Guid PeerId,
    int PeerCount);

public sealed record TextPublishedMessage(
    Guid SessionId,
    Guid PeerId,
    DateTimeOffset PublishedAtUtc,
    string Text);

public sealed record FileMetadataPublishedMessage(
    Guid SessionId,
    Guid PeerId,
    string FileName,
    string ContentType,
    long SizeBytes,
    DateTimeOffset PublishedAtUtc);
