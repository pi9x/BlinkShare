using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using BlinkShare.Api.Features.Shares.Unlock;
using BlinkShare.Api.Infrastructure.Persistence;
using BlinkShare.Api.Infrastructure.Security;
using BlinkShare.Api.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BlinkShare.Api.IntegrationTests.Features.Shares.Unlock;

public sealed class UnlockEndpointTests(BlinkShareApiFactory factory) : IClassFixture<BlinkShareApiFactory>
{
    [RequiresDockerFact]
    public async Task Valid_passcode_returns_unlock_proof()
    {
        await factory.ResetDatabaseAsync();
        var hasher = new Pbkdf2PasscodeHasher();

        await SeedShareAsync(new Share(
            Guid.NewGuid(),
            "LOCK0001",
            ShareTier.Free,
            ShareMode.StoredShare,
            ShareKind.Text,
            ShareStatus.Ready,
            null,
            hasher.Hash("secret"),
            "hidden",
            null,
            "text/plain; charset=utf-8",
            6,
            null,
            factory.FixedUtcNow.AddMinutes(-1),
            factory.FixedUtcNow.AddMinutes(5),
            null,
            0,
            null));

        using var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/v1/shares/LOCK0001/unlock", new Request("secret"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<Response>();

        Assert.NotNull(payload);
        Assert.Equal("LOCK0001", payload!.Code);
        Assert.False(string.IsNullOrWhiteSpace(payload.UnlockProof));
    }

    [RequiresDockerFact]
    public async Task Invalid_passcode_returns_403()
    {
        await factory.ResetDatabaseAsync();
        var hasher = new Pbkdf2PasscodeHasher();

        await SeedShareAsync(new Share(
            Guid.NewGuid(),
            "LOCK0001",
            ShareTier.Free,
            ShareMode.StoredShare,
            ShareKind.Text,
            ShareStatus.Ready,
            null,
            hasher.Hash("secret"),
            "hidden",
            null,
            "text/plain; charset=utf-8",
            6,
            null,
            factory.FixedUtcNow.AddMinutes(-1),
            factory.FixedUtcNow.AddMinutes(5),
            null,
            0,
            null));

        using var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/v1/shares/LOCK0001/unlock", new Request("wrong"));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);

        await using var contentStream = await response.Content.ReadAsStreamAsync();
        using var document = await JsonDocument.ParseAsync(contentStream);

        Assert.Equal("share.invalid_passcode", document.RootElement.GetProperty("code").GetString());
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
