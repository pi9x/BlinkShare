using System.Text.Json;
using StackExchange.Redis;

namespace BlinkShare.Api.Infrastructure.Redis;

public sealed class RedisPeerSessionStore(IConnectionMultiplexer connectionMultiplexer) : IPeerSessionStore
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    private readonly IDatabase _database = connectionMultiplexer.GetDatabase();

    public async Task CreateAsync(PeerSessionState session, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await SaveInternalAsync(session);
    }

    public async Task<PeerSessionState?> GetByCodeAsync(string code, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var sessionIdValue = await _database.StringGetAsync(GetCodeKey(code));
        if (!sessionIdValue.HasValue || !Guid.TryParse(sessionIdValue.ToString(), out var sessionId))
        {
            return null;
        }

        return await GetBySessionIdAsync(sessionId, cancellationToken);
    }

    public async Task<PeerSessionState?> GetBySessionIdAsync(Guid sessionId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var sessionValue = await _database.StringGetAsync(GetSessionKey(sessionId));
        if (!sessionValue.HasValue)
        {
            return null;
        }

        return JsonSerializer.Deserialize<PeerSessionState>(sessionValue.ToString(), SerializerOptions);
    }

    public async Task SaveAsync(PeerSessionState session, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await SaveInternalAsync(session);
    }

    public async Task RemoveAsync(Guid sessionId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var session = await GetBySessionIdAsync(sessionId, cancellationToken);
        if (session is null)
        {
            return;
        }

        var batch = _database.CreateBatch();
        var deleteSessionTask = batch.KeyDeleteAsync(GetSessionKey(sessionId));
        var deleteCodeTask = batch.KeyDeleteAsync(GetCodeKey(session.Code));
        batch.Execute();
        await Task.WhenAll(deleteSessionTask, deleteCodeTask);
    }

    private async Task SaveInternalAsync(PeerSessionState session)
    {
        var ttl = session.ExpiresAtUtc - DateTimeOffset.UtcNow;
        if (ttl <= TimeSpan.Zero)
        {
            ttl = TimeSpan.FromSeconds(1);
        }

        var serialized = JsonSerializer.Serialize(session, SerializerOptions);
        var batch = _database.CreateBatch();
        var saveSessionTask = batch.StringSetAsync(GetSessionKey(session.SessionId), serialized, ttl);
        var saveCodeTask = batch.StringSetAsync(GetCodeKey(session.Code), session.SessionId.ToString("D"), ttl);
        batch.Execute();
        await Task.WhenAll(saveSessionTask, saveCodeTask);
    }

    private static string GetSessionKey(Guid sessionId) => $"peer-session:state:{sessionId:D}";

    private static string GetCodeKey(string code) => $"peer-session:code:{code}";
}
