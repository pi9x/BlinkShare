namespace BlinkShare.Api.Infrastructure.Security;

public sealed class ShareUnlockProofOptions
{
    public int LifetimeMinutes { get; set; } = 5;
}
