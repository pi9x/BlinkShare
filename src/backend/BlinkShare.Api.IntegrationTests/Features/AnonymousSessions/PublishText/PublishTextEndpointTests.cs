using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using BlinkShare.Api.IntegrationTests.Infrastructure;
using CreateOrJoinRequest = BlinkShare.Api.Features.AnonymousSessions.CreateOrJoin.Request;
using CreateOrJoinResponse = BlinkShare.Api.Features.AnonymousSessions.CreateOrJoin.Response;
using PublishTextRequest = BlinkShare.Api.Features.AnonymousSessions.PublishText.Request;
using PublishTextResponse = BlinkShare.Api.Features.AnonymousSessions.PublishText.Response;

namespace BlinkShare.Api.IntegrationTests.Features.AnonymousSessions.PublishText;

public sealed class PublishTextEndpointTests(AnonymousSessionApiFactory factory) : IClassFixture<AnonymousSessionApiFactory>
{
    [Fact]
    public async Task Valid_peer_can_publish_text()
    {
        factory.ResetPeerSessions();
        using var client = factory.CreateClient();
        var createResponse = await client.PostAsJsonAsync("/api/v1/anonymous-sessions/join", new CreateOrJoinRequest(null));
        var created = await createResponse.Content.ReadFromJsonAsync<CreateOrJoinResponse>();

        var response = await client.PostAsJsonAsync(
            $"/api/v1/anonymous-sessions/{created!.SessionId}/text",
            new PublishTextRequest(created.PeerId, created.ResumeToken, "hello"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<PublishTextResponse>();

        Assert.NotNull(payload);
        Assert.Equal("hello", payload!.Text);
    }

    [Fact]
    public async Task Invalid_peer_returns_404()
    {
        factory.ResetPeerSessions();
        using var client = factory.CreateClient();
        var createResponse = await client.PostAsJsonAsync("/api/v1/anonymous-sessions/join", new CreateOrJoinRequest(null));
        var created = await createResponse.Content.ReadFromJsonAsync<CreateOrJoinResponse>();

        var response = await client.PostAsJsonAsync(
            $"/api/v1/anonymous-sessions/{created!.SessionId}/text",
            new PublishTextRequest(Guid.NewGuid(), created.ResumeToken, "hello"));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        await using var stream = await response.Content.ReadAsStreamAsync();
        using var document = await JsonDocument.ParseAsync(stream);

        Assert.Equal("session.peer_not_found", document.RootElement.GetProperty("code").GetString());
    }
}
