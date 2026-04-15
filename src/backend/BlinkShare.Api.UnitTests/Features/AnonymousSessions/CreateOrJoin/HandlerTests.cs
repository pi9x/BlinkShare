using BlinkShare.Api.Common.Time;
using BlinkShare.Api.Features.AnonymousSessions;
using BlinkShare.Api.Features.AnonymousSessions.CreateOrJoin;
using BlinkShare.Api.Infrastructure.Redis;
using BlinkShare.Api.Infrastructure.Security;
using Microsoft.Extensions.Options;

namespace BlinkShare.Api.UnitTests.Features.AnonymousSessions.CreateOrJoin;

public sealed class HandlerTests
{
    [Fact]
    public async Task Missing_code_creates_new_session()
    {
        var store = new TestPeerSessionStore();
        var handler = CreateHandler(store);

        var result = await handler.HandleAsync(new Command(null), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("TEST1234", result.Value!.Code);
        Assert.Equal(1, result.Value.PeerCount);
    }

    [Fact]
    public async Task Existing_code_joins_session()
    {
        var store = new TestPeerSessionStore();
        var createdAt = new DateTimeOffset(2026, 4, 12, 10, 30, 0, TimeSpan.Zero);
        await store.CreateAsync(
            new PeerSessionState(
                Guid.NewGuid(),
                "TEST1234",
                PeerSessionStatus.Waiting,
                createdAt,
                createdAt,
                60,
                [new PeerState(Guid.NewGuid(), "hash", createdAt, createdAt, createdAt.AddSeconds(60))]),
            CancellationToken.None);

        var handler = CreateHandler(store);

        var result = await handler.HandleAsync(new Command("TEST1234"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value!.PeerCount);
    }

    private static Handler CreateHandler(TestPeerSessionStore store) =>
        new(
            store,
            new Validator(),
            new FakeClock(new DateTimeOffset(2026, 4, 12, 10, 30, 0, TimeSpan.Zero)),
            new FixedCodeGenerator("TEST1234"),
            Options.Create(new AnonymousSessionOptions
            {
                ReconnectGraceSeconds = 60
            }));

    private sealed class FakeClock(DateTimeOffset utcNow) : IClock
    {
        public DateTimeOffset UtcNow { get; } = utcNow;
    }

    private sealed class FixedCodeGenerator(string code) : ICodeGenerator
    {
        public string GenerateShareCode() => code;
    }

    private sealed class TestPeerSessionStore : IPeerSessionStore
    {
        private readonly Dictionary<Guid, PeerSessionState> _sessions = [];
        private readonly Dictionary<string, Guid> _codes = new(StringComparer.Ordinal);

        public Task CreateAsync(PeerSessionState session, CancellationToken cancellationToken)
        {
            _sessions[session.SessionId] = session;
            _codes[session.Code] = session.SessionId;
            return Task.CompletedTask;
        }

        public Task<PeerSessionState?> GetByCodeAsync(string code, CancellationToken cancellationToken) =>
            Task.FromResult(_codes.TryGetValue(code, out var sessionId) ? _sessions[sessionId] : null);

        public Task<PeerSessionState?> GetBySessionIdAsync(Guid sessionId, CancellationToken cancellationToken) =>
            Task.FromResult(_sessions.TryGetValue(sessionId, out var session) ? session : null);

        public Task SaveAsync(PeerSessionState session, CancellationToken cancellationToken)
        {
            _sessions[session.SessionId] = session;
            _codes[session.Code] = session.SessionId;
            return Task.CompletedTask;
        }

        public Task RemoveAsync(Guid sessionId, CancellationToken cancellationToken)
        {
            if (_sessions.Remove(sessionId, out var session))
            {
                _codes.Remove(session.Code);
            }

            return Task.CompletedTask;
        }
    }
}
