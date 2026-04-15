using System.Net;
using System.Net.Http.Json;
using BlinkShare.Api.Features.Shares.CreateFileUpload;
using BlinkShare.Api.Infrastructure.Persistence;
using BlinkShare.Api.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BlinkShare.Api.IntegrationTests.Features.Shares.CreateFileUpload;

public sealed class CreateFileUploadEndpointTests(BlinkShareApiFactory factory) : IClassFixture<BlinkShareApiFactory>
{
    [RequiresDockerFact]
    public async Task Post_creates_pending_file_share()
    {
        await factory.ResetDatabaseAsync();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/v1/shares/file",
            new Request("report.txt", "text/plain", 12));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<Response>();

        Assert.NotNull(payload);
        Assert.Equal("TEST1234", payload!.Code);
        Assert.Contains("/upload/", payload.UploadUrl, StringComparison.Ordinal);

        await using var scope = factory.Services.CreateAsyncScope();
        var dbContextFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<BlinkShareDbContext>>();
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        var share = await dbContext.Shares.AsNoTracking().SingleAsync();

        Assert.Equal(ShareKind.File, share.Kind);
        Assert.Equal(ShareStatus.Pending, share.Status);
        Assert.Equal("report.txt", share.FileName);
    }

    [RequiresDockerFact]
    public async Task Post_rejects_anonymous_file_larger_than_limit()
    {
        await factory.ResetDatabaseAsync();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/v1/shares/file",
            new Request("large.bin", "application/octet-stream", (512 * 1024) + 1));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
