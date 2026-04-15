namespace BlinkShare.Api.Features.AnonymousSessions.PublishFileMetadata;

public sealed record Request(
    Guid PeerId,
    string? ResumeToken,
    string? FileName,
    string? ContentType,
    long SizeBytes,
    string? ShareCode = null)
{
    public Command ToCommand(Guid sessionId) => new(sessionId, PeerId, ResumeToken, FileName, ContentType, SizeBytes, ShareCode);
}
