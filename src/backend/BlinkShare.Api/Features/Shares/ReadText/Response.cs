namespace BlinkShare.Api.Features.Shares.ReadText;

public sealed record Response(
    Guid ShareId,
    string Code,
    string Text,
    DateTimeOffset? ExpiresAtUtc);
