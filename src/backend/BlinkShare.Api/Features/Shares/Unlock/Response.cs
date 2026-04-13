namespace BlinkShare.Api.Features.Shares.Unlock;

public sealed record Response(
    Guid ShareId,
    string Code,
    string UnlockProof,
    DateTimeOffset UnlockedUntilUtc);
