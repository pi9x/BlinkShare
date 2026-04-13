namespace BlinkShare.Api.Features.Shares.RequestDownload;

public sealed record Response(
    Guid ShareId,
    string Code,
    string DownloadUrl,
    int DownloadCount);
