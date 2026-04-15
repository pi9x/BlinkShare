using System.Security.Cryptography;
using System.Text;
using BlinkShare.Api.Common.Time;
using BlinkShare.Api.Features.AnonymousSessions;
using BlinkShare.Api.Features.AnonymousSessions.Resume;
using BlinkShare.Api.Infrastructure.Redis;

namespace BlinkShare.Api.UnitTests.Features.AnonymousSessions.Resume;

public sealed class HandlerTests
{
    [Fact]
    public async Task Invalid_resume_token_returns_failure()
    {
        var now = new DateTimeOffset(2026, 4, 12, 10, 30, 0, TimeSpan.Zero);
        var sessionId = Guid.NewGuid();
        var peerId = Guid.NewGuid();
        var store = new TestPeerSessionStore(new PeerSessionState(
            sessionId,
            "TEST1234",
            PeerSessionStatus.Active,
            now,
            now,
            60,
            [new PeerState(peerId, "HASH", now, now, now.AddSeconds(60))]));
        var handler = new Handler(new PeerSessionAccessService(store, new FakeClock(now)));

        var result = await handler.HandleAsync(new Request(sessionId, peerId, "wrong"), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("session.invalid_resume_token", result.Error!.Code);
    }

    [Fact]
    public async Task Elapsed_reconnect_grace_returns_failure()
    {
        var now = new DateTimeOffset(2026, 4, 12, 10, 30, 0, TimeSpan.Zero);
        var sessionId = Guid.NewGuid();
        var peerId = Guid.NewGuid();
        const string resumeToken = "resume-token";
        var store = new TestPeerSessionStore(new PeerSessionState(
            sessionId,
            "TEST1234",
            PeerSessionStatus.Active,
            now.AddMinutes(-10),
            now.AddMinutes(-1),
            60,
            [new PeerState(
                peerId,
                Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(resumeToken))),
                now.AddMinutes(-10),
                now.AddMinutes(-1),
                now.AddSeconds(-1))]));
        var handler = new Handler(new PeerSessionAccessService(store, new FakeClock(now)));

        var result = await handler.HandleAsync(new Request(sessionId, peerId, resumeToken), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("session.reconnect_grace_elapsed", result.Error!.Code);
    }

    private sealed class FakeClock(DateTimeOffset utcNow) : IClock
    {
        public DateTimeOffset UtcNow { get; } = utcNow;
    }

    private sealed class TestPeerSessionStore(PeerSessionState session) : IPeerSessionStore
    {
        public Task CreateAsync(PeerSessionState value, CancellationToken cancellationToken) => Task.CompletedTask;

        public Task<PeerSessionState?> GetByCodeAsync(string code, CancellationToken cancellationToken) =>
            Task.FromResult<PeerSessionState?>(session);

        public Task<PeerSessionState?> GetBySessionIdAsync(Guid sessionId, CancellationToken cancellationToken) =>
            Task.FromResult<PeerSessionState?>(session);

        public Task SaveAsync(PeerSessionState value, CancellationToken cancellationToken) => Task.CompletedTask;

        public Task RemoveAsync(Guid sessionId, CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
