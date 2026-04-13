using BlinkShare.Api.Infrastructure.Persistence;

namespace BlinkShare.Api.Features.Shares.GetByCode;

public sealed record Response(
    Guid ShareId,
    string Code,
    ShareKind Kind,
    ShareStatus Status,
    DateTimeOffset? ExpiresAtUtc,
    bool HasPasscode,
    long SizeBytes);
