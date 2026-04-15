namespace BlinkShare.Api.Features.Shares.CreateFileUpload;

public sealed record Request(
    string? FileName,
    string? ContentType,
    long SizeBytes)
{
    public Command ToCommand() => new(BlinkShare.Api.Infrastructure.Persistence.AccountTier.Anonymous, FileName, ContentType, SizeBytes);
}
