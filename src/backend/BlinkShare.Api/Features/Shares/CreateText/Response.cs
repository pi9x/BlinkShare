namespace BlinkShare.Api.Features.Shares.CreateText;

public sealed record Response(Guid ShareId, string Code, DateTimeOffset ExpiresAtUtc);
