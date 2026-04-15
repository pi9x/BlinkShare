namespace BlinkShare.Api.Features.AnonymousSessions;

public sealed class AnonymousSessionOptions
{
    public int ReconnectGraceSeconds { get; set; } = 60;

    public int MaxTextLength { get; set; } = 10_000;

    public long MaxFileSizeBytes { get; set; } = 10 * 1024 * 1024;
}
