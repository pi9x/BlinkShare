namespace BlinkShare.Api.Infrastructure.Redis;

public sealed record PeerState(
    Guid PeerId,
    string ResumeTokenHash,
    DateTimeOffset JoinedAtUtc,
    DateTimeOffset LastSeenAtUtc,
    DateTimeOffset ReconnectGraceEndsAtUtc)
{
    public bool IsReconnectValid(DateTimeOffset now) => ReconnectGraceEndsAtUtc >= now;

    public PeerState Touch(DateTimeOffset now, int reconnectGraceSeconds) =>
        this with
        {
            LastSeenAtUtc = now,
            ReconnectGraceEndsAtUtc = now.AddSeconds(reconnectGraceSeconds)
        };

    public PeerState MarkDisconnected(DateTimeOffset now, int reconnectGraceSeconds) =>
        this with
        {
            ReconnectGraceEndsAtUtc = now.AddSeconds(reconnectGraceSeconds)
        };
}
