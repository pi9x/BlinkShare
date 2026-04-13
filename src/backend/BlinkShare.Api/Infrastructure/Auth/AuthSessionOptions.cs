namespace BlinkShare.Api.Infrastructure.Auth;

public sealed class AuthSessionOptions
{
    public int SessionLifetimeHours { get; set; } = 24;
}
