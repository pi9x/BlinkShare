using BlinkShare.Api.Infrastructure.Persistence;

namespace BlinkShare.Api.Features.Auth.Register;

public sealed record Response(
    Guid AccountId,
    string Email,
    AccountTier Tier,
    string SessionToken,
    DateTimeOffset SessionExpiresAtUtc);
