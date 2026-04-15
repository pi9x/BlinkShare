using BlinkShare.Api.Infrastructure.Persistence;

namespace BlinkShare.Api.Common.Auth;

public sealed record CurrentAccount(Guid AccountId, string Email, AccountTier Tier);
