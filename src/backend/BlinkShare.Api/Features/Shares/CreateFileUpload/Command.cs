using BlinkShare.Api.Infrastructure.Persistence;

namespace BlinkShare.Api.Features.Shares.CreateFileUpload;

public sealed record Command(
    AccountTier Tier,
    string? FileName,
    string? ContentType,
    long SizeBytes);
