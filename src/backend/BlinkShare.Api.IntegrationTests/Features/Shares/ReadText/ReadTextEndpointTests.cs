using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using BlinkShare.Api.Features.Shares.ReadText;
using BlinkShare.Api.Infrastructure.Persistence;
using BlinkShare.Api.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BlinkShare.Api.IntegrationTests.Features.Shares.ReadText;

public sealed class ReadTextEndpointTests(BlinkShareApiFactory factory) : IClassFixture<BlinkShareApiFactory>
{
    [RequiresDockerFact]
    public async Task Existing_text_share_returns_text_payload()
    {
        await factory.ResetDatabaseAsync();
        var shareId = Guid.NewGuid();

        await SeedShareAsync(new Share(
            shareId,
            "READ0001",
            ShareMode.StoredShare,
            ShareKind.Text,
            ShareStatus.Ready,
            null,
            null,
            "hello",
            null,
            "text/plain; charset=utf-8",
            5,
            null,
            factory.FixedUtcNow.AddMinutes(-1),
            factory.FixedUtcNow.AddMinutes(5),
            null,
            0,
            null));

        using var client = factory.CreateClient();
        var response = await client.GetAsync("/api/v1/shares/READ0001/text");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<Response>();

        Assert.NotNull(payload);
        Assert.Equal(shareId, payload!.ShareId);
        Assert.Equal("hello", payload.Text);
    }

    [RequiresDockerFact]
    public async Task Expired_share_returns_410()
    {
        await factory.ResetDatabaseAsync();

        await SeedShareAsync(new Share(
            Guid.NewGuid(),
            "READ0001",
            ShareMode.StoredShare,
            ShareKind.Text,
            ShareStatus.Ready,
            null,
            null,
            "hello",
            null,
            "text/plain; charset=utf-8",
            5,
            null,
            factory.FixedUtcNow.AddMinutes(-10),
            factory.FixedUtcNow.AddMinutes(-1),
            null,
            0,
            null));

        using var client = factory.CreateClient();
        var response = await client.GetAsync("/api/v1/shares/READ0001/text");

        Assert.Equal(HttpStatusCode.Gone, response.StatusCode);
    }

    [RequiresDockerFact]
    public async Task Protected_share_without_unlock_proof_returns_403()
    {
        await factory.ResetDatabaseAsync();
        var hasher = new BlinkShare.Api.Infrastructure.Security.Pbkdf2PasscodeHasher();

        await SeedShareAsync(new Share(
            Guid.NewGuid(),
            "READ0001",
            ShareMode.StoredShare,
            ShareKind.Text,
            ShareStatus.Ready,
            null,
            hasher.Hash("secret"),
            "hello",
            null,
            "text/plain; charset=utf-8",
            5,
            null,
            factory.FixedUtcNow.AddMinutes(-1),
            factory.FixedUtcNow.AddMinutes(5),
            null,
            0,
            null));

        using var client = factory.CreateClient();
        var response = await client.GetAsync("/api/v1/shares/READ0001/text");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);

        await using var contentStream = await response.Content.ReadAsStreamAsync();
        using var document = await JsonDocument.ParseAsync(contentStream);

        Assert.Equal("share.passcode_required", document.RootElement.GetProperty("code").GetString());
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
