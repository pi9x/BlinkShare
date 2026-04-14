using System.ComponentModel.DataAnnotations;

namespace BlinkShare.Api.Infrastructure.ObjectStorage;

public sealed class ObjectStorageOptions
{
    [Required]
    [Url]
    public string ServiceUrl { get; set; } = "http://localhost:9000";

    [Required]
    [Url]
    public string PublicUrl { get; set; } = "http://localhost:9000";

    [Required]
    [MinLength(3)]
    [MaxLength(63)]
    public string BucketName { get; set; } = "blinkshare";

    [Required]
    public string AccessKey { get; set; } = "GK0123456789abcdef01234567";

    [Required]
    public string SecretKey { get; set; } = "0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef";

    [Required]
    public string Region { get; set; } = "garage";

    public bool ForcePathStyle { get; set; } = true;
}
