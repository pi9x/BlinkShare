namespace BlinkShare.Api.Features.Auth.Login;

public sealed record Response(
    Guid AccountId,
    string Email,
    string SessionToken,
    DateTimeOffset SessionExpiresAtUtc);
