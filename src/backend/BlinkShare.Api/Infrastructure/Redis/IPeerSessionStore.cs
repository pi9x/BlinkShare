namespace BlinkShare.Api.Infrastructure.Redis;

public interface IPeerSessionStore
{
    Task CreateAsync(PeerSessionState session, CancellationToken cancellationToken);

    Task<PeerSessionState?> GetByCodeAsync(string code, CancellationToken cancellationToken);

    Task<PeerSessionState?> GetBySessionIdAsync(Guid sessionId, CancellationToken cancellationToken);

    Task SaveAsync(PeerSessionState session, CancellationToken cancellationToken);

    Task RemoveAsync(Guid sessionId, CancellationToken cancellationToken);
}
