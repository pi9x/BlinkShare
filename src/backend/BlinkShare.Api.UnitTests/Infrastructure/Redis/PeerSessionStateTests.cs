using BlinkShare.Api.Infrastructure.Redis;

namespace BlinkShare.Api.UnitTests.Infrastructure.Redis;

public sealed class PeerSessionStateTests
{
    [Fact]
    public void Add_peer_increases_peer_count_and_updates_activity()
    {
        var createdAt = new DateTimeOffset(2026, 4, 12, 10, 0, 0, TimeSpan.Zero);
        var session = new PeerSessionState(
            Guid.NewGuid(),
            "TEST1234",
            PeerSessionStatus.Waiting,
            createdAt,
            createdAt.AddMinutes(5),
            createdAt,
            60,
            []);
        var peer = new PeerState(Guid.NewGuid(), "hash", createdAt, createdAt, createdAt.AddSeconds(60));

        var updated = session.AddPeer(peer, createdAt.AddSeconds(5));

        Assert.Equal(1, updated.PeerCount);
        Assert.Equal(PeerSessionStatus.Active, updated.Status);
        Assert.Equal(createdAt.AddSeconds(5), updated.LastActivityAtUtc);
    }

    [Fact]
    public void Touch_peer_refreshes_last_seen_and_reconnect_grace()
    {
        var createdAt = new DateTimeOffset(2026, 4, 12, 10, 0, 0, TimeSpan.Zero);
        var peerId = Guid.NewGuid();
        var session = new PeerSessionState(
            Guid.NewGuid(),
            "TEST1234",
            PeerSessionStatus.Waiting,
            createdAt,
            createdAt.AddMinutes(5),
            createdAt,
            60,
            [new PeerState(peerId, "hash", createdAt, createdAt, createdAt.AddSeconds(60))]);

        var updated = session.TouchPeer(peerId, createdAt.AddSeconds(10));
        var peer = updated.FindPeer(peerId);

        Assert.NotNull(peer);
        Assert.Equal(createdAt.AddSeconds(10), peer!.LastSeenAtUtc);
        Assert.Equal(createdAt.AddSeconds(70), peer.ReconnectGraceEndsAtUtc);
    }

    [Fact]
    public void Mark_peer_disconnected_refreshes_reconnect_grace_without_changing_last_seen()
    {
        var createdAt = new DateTimeOffset(2026, 4, 12, 10, 0, 0, TimeSpan.Zero);
        var peerId = Guid.NewGuid();
        var session = new PeerSessionState(
            Guid.NewGuid(),
            "TEST1234",
            PeerSessionStatus.Active,
            createdAt,
            createdAt.AddMinutes(5),
            createdAt,
            60,
            [new PeerState(peerId, "hash", createdAt, createdAt.AddSeconds(15), createdAt.AddSeconds(60))]);

        var updated = session.MarkPeerDisconnected(peerId, createdAt.AddMinutes(2));
        var peer = updated.FindPeer(peerId);

        Assert.NotNull(peer);
        Assert.Equal(createdAt.AddSeconds(15), peer!.LastSeenAtUtc);
        Assert.Equal(createdAt.AddMinutes(3), peer.ReconnectGraceEndsAtUtc);
    }
}
