namespace BlinkShare.Api.Features.AnonymousSessions.PublishText;

public sealed record Request(
    Guid PeerId,
    string? ResumeToken,
    string? Text)
{
    public Command ToCommand(Guid sessionId) => new(sessionId, PeerId, ResumeToken, Text);
}
