using BlinkShare.Api.Infrastructure.Persistence;

namespace BlinkShare.Api.Features.Auth.Me;

public sealed record Response(
    Guid AccountId,
    string Email,
    AccountTier Tier,
    DateTimeOffset CreatedAtUtc);
