using System.Net;
using System.Net.Http.Json;
using BlinkShare.Api.Features.Shares.RequestDownload;
using BlinkShare.Api.Infrastructure.Persistence;
using BlinkShare.Api.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BlinkShare.Api.IntegrationTests.Features.Shares.RequestDownload;

public sealed class RequestDownloadEndpointTests(BlinkShareApiFactory factory) : IClassFixture<BlinkShareApiFactory>
{
    [RequiresDockerFact]
    public async Task Ready_file_share_returns_download_url_and_updates_counter()
    {
        await factory.ResetDatabaseAsync();
        var shareId = Guid.NewGuid();

        await SeedShareAsync(new Share(
            shareId,
            "FILE0001",
            ShareTier.Free,
            ShareMode.StoredShare,
            ShareKind.File,
            ShareStatus.Ready,
            null,
            null,
            null,
            "report.txt",
            "text/plain",
            12,
            "shares/FILE0001/report.txt",
            factory.FixedUtcNow.AddMinutes(-1),
            factory.FixedUtcNow.AddMinutes(5),
            null,
            0,
            null));

        using var client = factory.CreateClient();
        var response = await client.PostAsync("/api/v1/shares/FILE0001/download", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<Response>();

        Assert.NotNull(payload);
        Assert.Equal(shareId, payload!.ShareId);
        Assert.Equal(1, payload.DownloadCount);
        Assert.Contains("/download/", payload.DownloadUrl, StringComparison.Ordinal);

        await using var scope = factory.Services.CreateAsyncScope();
        var dbContextFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<BlinkShareDbContext>>();
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        var share = await dbContext.Shares.AsNoTracking().SingleAsync(candidate => candidate.Code == "FILE0001");

        Assert.Equal(1, share.DownloadCount);
        Assert.Equal(factory.FixedUtcNow, share.LastAccessedAtUtc);
    }

    private async Task SeedShareAsync(Share share)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var dbContextFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<BlinkShareDbContext>>();
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        await dbContext.Shares.AddAsync(share);
        await dbContext.SaveChangesAsync();
    }
}
