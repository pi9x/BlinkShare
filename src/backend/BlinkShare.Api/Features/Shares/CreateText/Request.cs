using BlinkShare.Api.Infrastructure.Persistence;

namespace BlinkShare.Api.Features.Shares.CreateText;

public sealed record Request(ShareTier Tier, string? Text)
{
    public Command ToCommand() => new(Tier, Text);
}
