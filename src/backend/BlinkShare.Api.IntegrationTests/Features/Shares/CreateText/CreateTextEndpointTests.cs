using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using BlinkShare.Api.Features.Shares.CreateText;
using BlinkShare.Api.Infrastructure.Persistence;
using BlinkShare.Api.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RegisterRequest = BlinkShare.Api.Features.Auth.Register.Request;
using RegisterResponse = BlinkShare.Api.Features.Auth.Register.Response;

namespace BlinkShare.Api.IntegrationTests.Features.Shares.CreateText;

public sealed class CreateTextEndpointTests(BlinkShareApiFactory factory) : IClassFixture<BlinkShareApiFactory>
{
    [RequiresDockerFact]
    public async Task Post_creates_a_row_in_postgresql()
    {
        await factory.ResetDatabaseAsync();
        using var client = factory.CreateClient();
        await AuthenticateAsync(client);

        var response = await client.PostAsJsonAsync("/api/v1/shares/text", new Request("hello"));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<Response>();

        Assert.NotNull(payload);
        Assert.Equal("TEST1234", payload!.Code);
        Assert.Equal(factory.FixedUtcNow.AddMinutes(5), payload.ExpiresAtUtc);
        Assert.Equal(new Uri("/api/v1/shares/TEST1234", UriKind.Relative), response.Headers.Location);

        await using var scope = factory.Services.CreateAsyncScope();
        var dbContextFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<BlinkShareDbContext>>();
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        var share = await dbContext.Shares.AsNoTracking().SingleAsync();

        Assert.Equal(payload.ShareId, share.Id);
        Assert.Equal("TEST1234", share.Code);
        Assert.Equal("hello", share.TextInline);
        Assert.Equal(ShareKind.Text, share.Kind);
        Assert.Equal(ShareMode.StoredShare, share.Mode);
        Assert.Equal(ShareStatus.Ready, share.Status);
        Assert.Equal(factory.FixedUtcNow.AddMinutes(5), share.ExpiresAtUtc);
    }

    [RequiresDockerFact]
    public async Task Invalid_request_returns_400()
    {
        await factory.ResetDatabaseAsync();
        using var client = factory.CreateClient();
        await AuthenticateAsync(client);

        var response = await client.PostAsJsonAsync("/api/v1/shares/text", new Request(" "));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        await using var contentStream = await response.Content.ReadAsStreamAsync();
        using var document = await JsonDocument.ParseAsync(contentStream);

        Assert.Equal("share.invalid_text", document.RootElement.GetProperty("code").GetString());
    }

    private static async Task AuthenticateAsync(HttpClient client)
    {
        var response = await client.PostAsJsonAsync(
            "/api/v1/auth/register",
            new RegisterRequest($"{Guid.NewGuid():N}@example.com", string.Empty));
        var payload = await response.Content.ReadFromJsonAsync<RegisterResponse>();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", payload!.SessionToken);
    }
}
