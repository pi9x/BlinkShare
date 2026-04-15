using BlinkShare.Api.Infrastructure.Persistence;

namespace BlinkShare.Api.Features.Shares.CreateText;

public sealed record Command(AccountTier Tier, string? Text);
