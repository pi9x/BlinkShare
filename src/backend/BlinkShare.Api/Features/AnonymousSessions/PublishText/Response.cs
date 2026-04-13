namespace BlinkShare.Api.Features.AnonymousSessions.PublishText;

public sealed record Response(
    Guid SessionId,
    Guid PeerId,
    DateTimeOffset PublishedAtUtc,
    string Text);
