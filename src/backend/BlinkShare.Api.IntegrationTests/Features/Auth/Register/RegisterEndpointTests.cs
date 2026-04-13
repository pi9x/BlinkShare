using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using BlinkShare.Api.Features.Auth.Register;
using BlinkShare.Api.IntegrationTests.Infrastructure;

namespace BlinkShare.Api.IntegrationTests.Features.Auth.Register;

public sealed class RegisterEndpointTests(BlinkShareApiFactory factory) : IClassFixture<BlinkShareApiFactory>
{
    [RequiresDockerFact]
    public async Task Post_registers_account_successfully()
    {
        await factory.ResetDatabaseAsync();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/v1/auth/register", new Request("demo@example.com", string.Empty));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [RequiresDockerFact]
    public async Task Duplicate_email_returns_409()
    {
        await factory.ResetDatabaseAsync();
        using var client = factory.CreateClient();

        await client.PostAsJsonAsync("/api/v1/auth/register", new Request("demo@example.com", string.Empty));
        var duplicateResponse = await client.PostAsJsonAsync("/api/v1/auth/register", new Request("demo@example.com", string.Empty));

        Assert.Equal(HttpStatusCode.Conflict, duplicateResponse.StatusCode);

        await using var stream = await duplicateResponse.Content.ReadAsStreamAsync();
        using var document = await JsonDocument.ParseAsync(stream);

        Assert.Equal("auth.duplicate_email", document.RootElement.GetProperty("code").GetString());
    }
}
