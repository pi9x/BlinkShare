namespace BlinkShare.Api.Features.Shares.CreateText;

public sealed record Request(string? Text)
{
    public Command ToCommand() => new(BlinkShare.Api.Infrastructure.Persistence.AccountTier.Anonymous, Text);
}
