using BlinkShare.Api.Infrastructure.Persistence;

namespace BlinkShare.Api.Features.Shares.CreateText;

public sealed record Command(ShareTier Tier, string? Text);
