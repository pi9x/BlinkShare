namespace BlinkShare.Api.Features.AnonymousSessions.Resume;

public sealed record Request(
    Guid SessionId,
    Guid PeerId,
    string? ResumeToken);
