using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using BlinkShare.Api.IntegrationTests.Infrastructure;
using CreateOrJoinRequest = BlinkShare.Api.Features.AnonymousSessions.CreateOrJoin.Request;
using CreateOrJoinResponse = BlinkShare.Api.Features.AnonymousSessions.CreateOrJoin.Response;
using ResumeRequest = BlinkShare.Api.Features.AnonymousSessions.Resume.Request;

namespace BlinkShare.Api.IntegrationTests.Features.AnonymousSessions.Resume;

public sealed class ResumeEndpointTests(AnonymousSessionApiFactory factory) : IClassFixture<AnonymousSessionApiFactory>
{
    [Fact]
    public async Task Valid_resume_returns_200()
    {
        factory.ResetPeerSessions();
        using var client = factory.CreateClient();
        var createResponse = await client.PostAsJsonAsync("/api/v1/anonymous-sessions/join", new CreateOrJoinRequest(null));
        var created = await createResponse.Content.ReadFromJsonAsync<CreateOrJoinResponse>();

        var response = await client.PostAsJsonAsync(
            "/api/v1/anonymous-sessions/resume",
            new ResumeRequest(created!.SessionId, created.PeerId, created.ResumeToken));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Invalid_resume_token_returns_403()
    {
        factory.ResetPeerSessions();
        using var client = factory.CreateClient();
        var createResponse = await client.PostAsJsonAsync("/api/v1/anonymous-sessions/join", new CreateOrJoinRequest(null));
        var created = await createResponse.Content.ReadFromJsonAsync<CreateOrJoinResponse>();

        var response = await client.PostAsJsonAsync(
            "/api/v1/anonymous-sessions/resume",
            new ResumeRequest(created!.SessionId, created.PeerId, "wrong"));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);

        await using var stream = await response.Content.ReadAsStreamAsync();
        using var document = await JsonDocument.ParseAsync(stream);

        Assert.Equal("session.invalid_resume_token", document.RootElement.GetProperty("code").GetString());
    }
}
