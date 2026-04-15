using BlinkShare.Api.Infrastructure.Persistence;

namespace BlinkShare.Api.Features.Quotas.GetUsage;

public sealed record Response(
    AccountTier Tier,
    long BytesUsedToday,
    long BytesLimitToday,
    int SharesCreatedToday,
    DateTimeOffset WindowEndsAtUtc);
