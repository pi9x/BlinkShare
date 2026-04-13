using BlinkShare.Api.Common.Time;
using BlinkShare.Api.Features.AnonymousSessions;
using BlinkShare.Api.Features.AnonymousSessions.PublishFileMetadata;
using BlinkShare.Api.Infrastructure.Redis;
using Microsoft.Extensions.Options;

namespace BlinkShare.Api.UnitTests.Features.AnonymousSessions.PublishFileMetadata;

public sealed class HandlerTests
{
    [Fact]
    public async Task Invalid_metadata_returns_failure()
    {
        var handler = CreateHandler(new TestPeerSessionStore(null));

        var result = await handler.HandleAsync(new Command(Guid.NewGuid(), Guid.NewGuid(), "token", null, "text/plain", 1), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("general.validation", result.Error!.Code);
    }

    [Fact]
    public async Task Missing_session_returns_failure()
    {
        var handler = CreateHandler(new TestPeerSessionStore(null));

        var result = await handler.HandleAsync(new Command(Guid.NewGuid(), Guid.NewGuid(), "token", "file.txt", "text/plain", 1), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("session.not_found", result.Error!.Code);
    }

    private static Handler CreateHandler(IPeerSessionStore store) =>
        new(
            new PeerSessionAccessService(
                store,
                new FakeClock(new DateTimeOffset(2026, 4, 12, 10, 30, 0, TimeSpan.Zero))),
            new Validator(Options.Create(new AnonymousSessionOptions
            {
                MaxFileSizeBytes = 1000
            })));

    private sealed class FakeClock(DateTimeOffset utcNow) : IClock
    {
        public DateTimeOffset UtcNow { get; } = utcNow;
    }

    private sealed class TestPeerSessionStore(PeerSessionState? session) : IPeerSessionStore
    {
        public Task CreateAsync(PeerSessionState value, CancellationToken cancellationToken) => Task.CompletedTask;

        public Task<PeerSessionState?> GetByCodeAsync(string code, CancellationToken cancellationToken) =>
            Task.FromResult(session);

        public Task<PeerSessionState?> GetBySessionIdAsync(Guid sessionId, CancellationToken cancellationToken) =>
            Task.FromResult(session);

        public Task SaveAsync(PeerSessionState value, CancellationToken cancellationToken) => Task.CompletedTask;

        public Task RemoveAsync(Guid sessionId, CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
