namespace BlinkShare.Api.Features.Shares.CreateFileUpload;

public sealed class CreateFileUploadOptions
{
    public long MaxFileSizeBytes { get; set; } = 10 * 1024 * 1024;

    public int FreeTierTtlMinutes { get; set; } = 5;
}
