using BlinkShare.Api.Infrastructure.Persistence;

namespace BlinkShare.Api.Features.Quotas.GetUsage;

public sealed record Response(
    ShareTier Tier,
    long BytesUsedToday,
    long BytesLimitToday,
    int SharesCreatedToday,
    DateTimeOffset WindowEndsAtUtc);
