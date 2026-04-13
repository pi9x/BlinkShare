using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using BlinkShare.Api.Features.Shares.GetByCode;
using BlinkShare.Api.Infrastructure.Persistence;
using BlinkShare.Api.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BlinkShare.Api.IntegrationTests.Features.Shares.GetByCode;

public sealed class GetByCodeEndpointTests(BlinkShareApiFactory factory) : IClassFixture<BlinkShareApiFactory>
{
    [RequiresDockerFact]
    public async Task Existing_share_returns_public_metadata()
    {
        await factory.ResetDatabaseAsync();
        var shareId = Guid.NewGuid();

        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var dbContextFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<BlinkShareDbContext>>();
            await using var dbContext = await dbContextFactory.CreateDbContextAsync();
            await dbContext.Shares.AddAsync(new Share(
                id: shareId,
                code: "FETCH001",
                tier: ShareTier.Free,
                mode: ShareMode.StoredShare,
                kind: ShareKind.Text,
                status: ShareStatus.Ready,
                ownerUserId: null,
                passcodeHash: "hash",
                textInline: "hello",
                fileName: null,
                contentType: "text/plain; charset=utf-8",
                sizeBytes: 5,
                storageKey: null,
                createdAtUtc: factory.FixedUtcNow.AddMinutes(-1),
                expiresAtUtc: factory.FixedUtcNow.AddMinutes(5),
                lastAccessedAtUtc: null,
                downloadCount: 0,
                maxDownloadCount: null));
            await dbContext.SaveChangesAsync();
        }

        using var client = factory.CreateClient();
        var response = await client.GetAsync("/api/v1/shares/FETCH001");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<Response>();

        Assert.NotNull(payload);
        Assert.Equal(shareId, payload!.ShareId);
        Assert.Equal("FETCH001", payload.Code);
        Assert.Equal(ShareKind.Text, payload.Kind);
        Assert.Equal(ShareStatus.Ready, payload.Status);
        Assert.Equal(factory.FixedUtcNow.AddMinutes(5), payload.ExpiresAtUtc);
        Assert.True(payload.HasPasscode);
        Assert.Equal(5, payload.SizeBytes);
    }

    [RequiresDockerFact]
    public async Task Missing_share_returns_404()
    {
        await factory.ResetDatabaseAsync();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/v1/shares/MISSING1");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        await using var contentStream = await response.Content.ReadAsStreamAsync();
        using var document = await JsonDocument.ParseAsync(contentStream);

        Assert.Equal("share.not_found", document.RootElement.GetProperty("code").GetString());
    }
}
