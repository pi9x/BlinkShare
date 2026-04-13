namespace BlinkShare.Api.Infrastructure.Redis;

public sealed record PeerSessionState(
    Guid SessionId,
    string Code,
    PeerSessionStatus Status,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset ExpiresAtUtc,
    DateTimeOffset LastActivityAtUtc,
    int ReconnectGraceSeconds,
    IReadOnlyList<PeerState> Peers)
{
    public int PeerCount => Peers.Count;

    public bool IsExpired(DateTimeOffset now) =>
        ExpiresAtUtc <= now || Status is PeerSessionStatus.Expired or PeerSessionStatus.Closed;

    public PeerState? FindPeer(Guid peerId) => Peers.SingleOrDefault(peer => peer.PeerId == peerId);

    public PeerSessionState AddPeer(PeerState peer, DateTimeOffset now) =>
        this with
        {
            Status = PeerSessionStatus.Active,
            LastActivityAtUtc = now,
            Peers = [.. Peers, peer]
        };

    public PeerSessionState TouchPeer(Guid peerId, DateTimeOffset now)
    {
        var peers = Peers
            .Select(peer => peer.PeerId == peerId ? peer.Touch(now, ReconnectGraceSeconds) : peer)
            .ToArray();

        return this with
        {
            Status = PeerSessionStatus.Active,
            LastActivityAtUtc = now,
            Peers = peers
        };
    }

    public PeerSessionState WithActivity(Guid peerId, DateTimeOffset now) => TouchPeer(peerId, now);

    public PeerSessionState MarkPeerDisconnected(Guid peerId, DateTimeOffset now)
    {
        var peers = Peers
            .Select(peer => peer.PeerId == peerId ? peer.MarkDisconnected(now, ReconnectGraceSeconds) : peer)
            .ToArray();

        return this with
        {
            Peers = peers
        };
    }

    public PeerSessionState Expire() => this with { Status = PeerSessionStatus.Expired };
}
