using System.Net;
using System.Net.Http.Json;
using BlinkShare.Api.Features.Quotas.GetUsage;
using BlinkShare.Api.IntegrationTests.Infrastructure;

namespace BlinkShare.Api.IntegrationTests.Features.Quotas.GetUsage;

public sealed class GetUsageEndpointTests(AnonymousSessionApiFactory factory) : IClassFixture<AnonymousSessionApiFactory>
{
    [Fact]
    public async Task Get_returns_usage_payload()
    {
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/v1/quotas/usage");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<Response>();

        Assert.NotNull(payload);
    }
}
