using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using BlinkShare.Api.Features.Shares.CompleteFileUpload;
using BlinkShare.Api.Infrastructure.Persistence;
using BlinkShare.Api.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BlinkShare.Api.IntegrationTests.Features.Shares.CompleteFileUpload;

public sealed class CompleteFileUploadEndpointTests(BlinkShareApiFactory factory) : IClassFixture<BlinkShareApiFactory>
{
    [RequiresDockerFact]
    public async Task Pending_file_share_transitions_to_ready()
    {
        await factory.ResetDatabaseAsync();

        await SeedShareAsync(new Share(
            Guid.NewGuid(),
            "FILE0001",
            ShareTier.Free,
            ShareMode.StoredShare,
            ShareKind.File,
            ShareStatus.Pending,
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
        var response = await client.PostAsync("/api/v1/shares/FILE0001/file/complete", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<Response>();

        Assert.NotNull(payload);
        Assert.Equal(ShareStatus.Ready, payload!.Status);
    }

    [RequiresDockerFact]
    public async Task Ready_file_share_returns_409()
    {
        await factory.ResetDatabaseAsync();

        await SeedShareAsync(new Share(
            Guid.NewGuid(),
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
        var response = await client.PostAsync("/api/v1/shares/FILE0001/file/complete", null);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);

        await using var contentStream = await response.Content.ReadAsStreamAsync();
        using var document = await JsonDocument.ParseAsync(contentStream);

        Assert.Equal("share.invalid_status", document.RootElement.GetProperty("code").GetString());
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
