using System.Net;
using System.Net.Http.Json;
using BlinkShare.Api.IntegrationTests.Infrastructure;

namespace BlinkShare.Api.IntegrationTests.Features.Health;

public sealed class HealthEndpointTests(AnonymousSessionApiFactory factory) : IClassFixture<AnonymousSessionApiFactory>
{
    [Fact]
    public async Task Get_healthz_returns_ok_payload()
    {
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/healthz");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<HealthResponse>();

        Assert.NotNull(payload);
        Assert.Equal("ok", payload.Status);
    }

    private sealed record HealthResponse(string Status);
}
