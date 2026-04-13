namespace BlinkShare.Api.Infrastructure.Persistence;

public sealed class AuthSession
{
    private AuthSession()
    {
    }

    public AuthSession(
        Guid id,
        Guid accountId,
        string tokenHash,
        DateTimeOffset createdAtUtc,
        DateTimeOffset expiresAtUtc,
        DateTimeOffset? lastSeenAtUtc,
        DateTimeOffset? revokedAtUtc)
    {
        Id = id;
        AccountId = accountId;
        TokenHash = tokenHash;
        CreatedAtUtc = createdAtUtc;
        ExpiresAtUtc = expiresAtUtc;
        LastSeenAtUtc = lastSeenAtUtc;
        RevokedAtUtc = revokedAtUtc;
    }

    public Guid Id { get; private set; }

    public Guid AccountId { get; private set; }

    public string TokenHash { get; private set; } = string.Empty;

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset ExpiresAtUtc { get; private set; }

    public DateTimeOffset? LastSeenAtUtc { get; private set; }

    public DateTimeOffset? RevokedAtUtc { get; private set; }

    public void MarkSeen(DateTimeOffset seenAtUtc)
    {
        LastSeenAtUtc = seenAtUtc;
    }

    public void Revoke(DateTimeOffset revokedAtUtc)
    {
        RevokedAtUtc = revokedAtUtc;
    }
}
