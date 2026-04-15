namespace BlinkShare.Api.Infrastructure.Persistence;

public sealed class Account
{
    private Account()
    {
    }

    public Account(
        Guid id,
        string email,
        string normalizedEmail,
        string passwordHash,
        AccountTier tier,
        DateTimeOffset createdAtUtc,
        DateTimeOffset? lastLoginAtUtc)
    {
        Id = id;
        Email = email;
        NormalizedEmail = normalizedEmail;
        PasswordHash = passwordHash;
        Tier = tier;
        CreatedAtUtc = createdAtUtc;
        LastLoginAtUtc = lastLoginAtUtc;
    }

    public Guid Id { get; private set; }

    public string Email { get; private set; } = string.Empty;

    public string NormalizedEmail { get; private set; } = string.Empty;

    public string PasswordHash { get; private set; } = string.Empty;

    public AccountTier Tier { get; private set; } = AccountTier.Free;

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset? LastLoginAtUtc { get; private set; }
}
