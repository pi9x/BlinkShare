using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using BlinkShare.Api.IntegrationTests.Infrastructure;
using LoginRequest = BlinkShare.Api.Features.Auth.Login.Request;
using LoginResponse = BlinkShare.Api.Features.Auth.Login.Response;
using MeResponse = BlinkShare.Api.Features.Auth.Me.Response;
using RegisterRequest = BlinkShare.Api.Features.Auth.Register.Request;

namespace BlinkShare.Api.IntegrationTests.Features.Auth.Login;

public sealed class LoginAndMeEndpointTests(BlinkShareApiFactory factory) : IClassFixture<BlinkShareApiFactory>
{
    [RequiresDockerFact]
    public async Task Login_returns_session_and_me_requires_authentication()
    {
        await factory.ResetDatabaseAsync();
        using var client = factory.CreateClient();

        await client.PostAsJsonAsync("/api/v1/auth/register", new RegisterRequest("demo@example.com", string.Empty));

        var loginResponse = await client.PostAsJsonAsync("/api/v1/auth/login", new LoginRequest("demo@example.com", string.Empty));

        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        var loginPayload = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(loginPayload);

        using var unauthorizedClient = factory.CreateClient();
        var unauthorizedMe = await unauthorizedClient.GetAsync("/api/v1/auth/me");
        Assert.Equal(HttpStatusCode.Unauthorized, unauthorizedMe.StatusCode);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginPayload!.SessionToken);
        var meResponse = await client.GetAsync("/api/v1/auth/me");

        Assert.Equal(HttpStatusCode.OK, meResponse.StatusCode);

        var mePayload = await meResponse.Content.ReadFromJsonAsync<MeResponse>();
        Assert.NotNull(mePayload);
        Assert.Equal("demo@example.com", mePayload!.Email);
    }
}
