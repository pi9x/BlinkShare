namespace BlinkShare.Api.Features.Shares.CreateFileUpload;

public sealed class CreateFileUploadOptions
{
    public long AnonymousMaxFileSizeBytes { get; set; } = 512 * 1024;

    public long FreeMaxFileSizeBytes { get; set; } = 1024 * 1024;

    public int FreeTierTtlMinutes { get; set; } = 5;
}
