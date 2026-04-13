namespace BlinkShare.Api.Features.Auth.Register;

public sealed record Response(
    Guid AccountId,
    string Email,
    string SessionToken,
    DateTimeOffset SessionExpiresAtUtc);
