using System.Net;
using System.Net.Http.Json;
using BlinkShare.Api.Features.AnonymousSessions.CreateOrJoin;
using BlinkShare.Api.IntegrationTests.Infrastructure;

namespace BlinkShare.Api.IntegrationTests.Features.AnonymousSessions.CreateOrJoin;

public sealed class CreateOrJoinEndpointTests(AnonymousSessionApiFactory factory) : IClassFixture<AnonymousSessionApiFactory>
{
    [Fact]
    public async Task Missing_code_creates_new_session()
    {
        factory.ResetPeerSessions();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/v1/anonymous-sessions/join", new Request(null));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<Response>();

        Assert.NotNull(payload);
        Assert.Equal("TEST1234", payload!.Code);
        Assert.Equal(1, payload.PeerCount);
    }

    [Fact]
    public async Task Existing_code_joins_existing_session()
    {
        factory.ResetPeerSessions();
        using var client = factory.CreateClient();
        var createResponse = await client.PostAsJsonAsync("/api/v1/anonymous-sessions/join", new Request(null));
        var created = await createResponse.Content.ReadFromJsonAsync<Response>();

        var joinResponse = await client.PostAsJsonAsync("/api/v1/anonymous-sessions/join", new Request(created!.Code));

        Assert.Equal(HttpStatusCode.OK, joinResponse.StatusCode);

        var joined = await joinResponse.Content.ReadFromJsonAsync<Response>();

        Assert.NotNull(joined);
        Assert.Equal(2, joined!.PeerCount);
        Assert.Equal(created.SessionId, joined.SessionId);
    }
}
