using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using BlinkShare.Api.IntegrationTests.Infrastructure;
using CreateOrJoinRequest = BlinkShare.Api.Features.AnonymousSessions.CreateOrJoin.Request;
using CreateOrJoinResponse = BlinkShare.Api.Features.AnonymousSessions.CreateOrJoin.Response;
using PublishFileMetadataRequest = BlinkShare.Api.Features.AnonymousSessions.PublishFileMetadata.Request;
using PublishFileMetadataResponse = BlinkShare.Api.Features.AnonymousSessions.PublishFileMetadata.Response;

namespace BlinkShare.Api.IntegrationTests.Features.AnonymousSessions.PublishFileMetadata;

public sealed class PublishFileMetadataEndpointTests(AnonymousSessionApiFactory factory) : IClassFixture<AnonymousSessionApiFactory>
{
    [Fact]
    public async Task Valid_peer_can_publish_file_metadata()
    {
        factory.ResetPeerSessions();
        using var client = factory.CreateClient();
        var createResponse = await client.PostAsJsonAsync("/api/v1/anonymous-sessions/join", new CreateOrJoinRequest(null));
        var created = await createResponse.Content.ReadFromJsonAsync<CreateOrJoinResponse>();

        var response = await client.PostAsJsonAsync(
            $"/api/v1/anonymous-sessions/{created!.SessionId}/file-metadata",
            new PublishFileMetadataRequest(created.PeerId, created.ResumeToken, "report.txt", "text/plain", 12));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<PublishFileMetadataResponse>();

        Assert.NotNull(payload);
        Assert.Equal("report.txt", payload!.FileName);
    }

    [Fact]
    public async Task Invalid_metadata_returns_400()
    {
        factory.ResetPeerSessions();
        using var client = factory.CreateClient();
        var createResponse = await client.PostAsJsonAsync("/api/v1/anonymous-sessions/join", new CreateOrJoinRequest(null));
        var created = await createResponse.Content.ReadFromJsonAsync<CreateOrJoinResponse>();

        var response = await client.PostAsJsonAsync(
            $"/api/v1/anonymous-sessions/{created!.SessionId}/file-metadata",
            new PublishFileMetadataRequest(created.PeerId, created.ResumeToken, null, "text/plain", 12));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        await using var stream = await response.Content.ReadAsStreamAsync();
        using var document = await JsonDocument.ParseAsync(stream);

        Assert.Equal("general.validation", document.RootElement.GetProperty("code").GetString());
    }
}
