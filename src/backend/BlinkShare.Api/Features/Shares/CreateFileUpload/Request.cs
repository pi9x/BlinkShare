using BlinkShare.Api.Infrastructure.Persistence;

namespace BlinkShare.Api.Features.Shares.CreateFileUpload;

public sealed record Request(
    ShareTier Tier,
    string? FileName,
    string? ContentType,
    long SizeBytes)
{
    public Command ToCommand() => new(Tier, FileName, ContentType, SizeBytes);
}
