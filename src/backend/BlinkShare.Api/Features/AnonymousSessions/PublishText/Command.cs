namespace BlinkShare.Api.Features.AnonymousSessions.PublishText;

public sealed record Command(
    Guid SessionId,
    Guid PeerId,
    string? ResumeToken,
    string? Text);
