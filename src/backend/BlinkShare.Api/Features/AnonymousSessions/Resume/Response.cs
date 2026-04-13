namespace BlinkShare.Api.Features.AnonymousSessions.Resume;

public sealed record Response(
    Guid SessionId,
    string Code,
    Guid PeerId,
    int PeerCount,
    DateTimeOffset ReconnectedAtUtc);
