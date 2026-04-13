using BlinkShare.Api.Common.Time;
using BlinkShare.Api.Infrastructure.Redis;
using BlinkShare.Api.Infrastructure.Security;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BlinkShare.Api.IntegrationTests.Infrastructure;

public sealed class AnonymousSessionApiFactory : WebApplicationFactory<Program>
{
    public DateTimeOffset FixedUtcNow { get; } = new(2026, 4, 12, 10, 30, 0, TimeSpan.Zero);

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseContentRoot(TestPaths.GetApiContentRoot());

        builder.ConfigureAppConfiguration((_, configurationBuilder) =>
        {
            configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["AnonymousSessions:SessionLifetimeMinutes"] = "5",
                ["AnonymousSessions:ReconnectGraceSeconds"] = "60",
                ["AnonymousSessions:MaxTextLength"] = "1000",
                ["AnonymousSessions:MaxFileSizeBytes"] = "2048"
            });
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IClock>();
            services.AddSingleton<IClock>(new FixedClock(FixedUtcNow));

            services.RemoveAll<ICodeGenerator>();
            services.AddSingleton<ICodeGenerator>(new FixedCodeGenerator("TEST1234"));

            services.RemoveAll<IPeerSessionStore>();
            services.AddSingleton<IPeerSessionStore, InMemoryPeerSessionStore>();
        });
    }

    public void ResetPeerSessions()
    {
        using var scope = Services.CreateScope();
        var store = (InMemoryPeerSessionStore)scope.ServiceProvider.GetRequiredService<IPeerSessionStore>();
        store.Clear();
    }

    private sealed class FixedClock(DateTimeOffset utcNow) : IClock
    {
        public DateTimeOffset UtcNow { get; } = utcNow;
    }

    private sealed class FixedCodeGenerator(string code) : ICodeGenerator
    {
        public string GenerateShareCode() => code;
    }
}
