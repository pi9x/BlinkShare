using BlinkShare.Api.Infrastructure.Persistence;

namespace BlinkShare.Api.Features.Shares.CreateFileUpload;

public sealed record Command(
    ShareTier Tier,
    string? FileName,
    string? ContentType,
    long SizeBytes);
