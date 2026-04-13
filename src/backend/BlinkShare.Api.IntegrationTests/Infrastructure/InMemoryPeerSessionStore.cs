using BlinkShare.Api.Infrastructure.Redis;

namespace BlinkShare.Api.IntegrationTests.Infrastructure;

public sealed class InMemoryPeerSessionStore : IPeerSessionStore
{
    private readonly Dictionary<Guid, PeerSessionState> _sessions = [];
    private readonly Dictionary<string, Guid> _codes = new(StringComparer.Ordinal);
    private readonly Lock _lock = new();

    public Task CreateAsync(PeerSessionState session, CancellationToken cancellationToken)
    {
        lock (_lock)
        {
            _sessions[session.SessionId] = session;
            _codes[session.Code] = session.SessionId;
        }

        return Task.CompletedTask;
    }

    public Task<PeerSessionState?> GetByCodeAsync(string code, CancellationToken cancellationToken)
    {
        lock (_lock)
        {
            return Task.FromResult(_codes.TryGetValue(code, out var sessionId) ? _sessions[sessionId] : null);
        }
    }

    public Task<PeerSessionState?> GetBySessionIdAsync(Guid sessionId, CancellationToken cancellationToken)
    {
        lock (_lock)
        {
            return Task.FromResult(_sessions.TryGetValue(sessionId, out var session) ? session : null);
        }
    }

    public Task SaveAsync(PeerSessionState session, CancellationToken cancellationToken)
    {
        lock (_lock)
        {
            _sessions[session.SessionId] = session;
            _codes[session.Code] = session.SessionId;
        }

        return Task.CompletedTask;
    }

    public Task RemoveAsync(Guid sessionId, CancellationToken cancellationToken)
    {
        lock (_lock)
        {
            if (_sessions.Remove(sessionId, out var session))
            {
                _codes.Remove(session.Code);
            }
        }

        return Task.CompletedTask;
    }

    public void Clear()
    {
        lock (_lock)
        {
            _sessions.Clear();
            _codes.Clear();
        }
    }
}
