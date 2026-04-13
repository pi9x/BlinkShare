namespace BlinkShare.Api.Features.AnonymousSessions.CreateOrJoin;

public sealed record Response(
    Guid SessionId,
    string Code,
    Guid PeerId,
    string ResumeToken,
    int PeerCount,
    int ReconnectGraceSeconds);
