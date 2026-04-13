using BlinkShare.Api.Infrastructure.ObjectStorage;
using Microsoft.Extensions.Options;

namespace BlinkShare.Api.UnitTests.Infrastructure.ObjectStorage;

public sealed class DevelopmentObjectStorageTests
{
    [Fact]
    public async Task Create_upload_target_returns_deterministic_placeholder_url()
    {
        var storage = CreateStorage();

        var result = await storage.CreateUploadTargetAsync(
            new ObjectStorageUploadRequest("shares/ABC123/file.txt", "text/plain", 12, new DateTimeOffset(2026, 4, 12, 11, 0, 0, TimeSpan.Zero)),
            CancellationToken.None);

        Assert.Equal("shares/ABC123/file.txt", result.StorageKey);
        Assert.Equal("PUT", result.HttpMethod);
        Assert.StartsWith("https://object-storage.test/upload/", result.UploadUrl, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Delete_object_marks_key_as_deleted()
    {
        var storage = CreateStorage();

        await storage.DeleteObjectAsync("shares/ABC123/file.txt", CancellationToken.None);

        Assert.True(storage.WasDeleted("shares/ABC123/file.txt"));
    }

    private static DevelopmentObjectStorage CreateStorage() =>
        new(Options.Create(new ObjectStorageOptions
        {
            BaseUrl = "https://object-storage.test"
        }));
}
