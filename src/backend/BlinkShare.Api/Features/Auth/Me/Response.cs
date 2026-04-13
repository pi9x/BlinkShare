namespace BlinkShare.Api.Features.Auth.Me;

public sealed record Response(
    Guid AccountId,
    string Email,
    DateTimeOffset CreatedAtUtc);
