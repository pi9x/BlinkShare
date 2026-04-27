using BlinkShare.Api.Common.Time;
using BlinkShare.Api.Infrastructure.ObjectStorage;
using BlinkShare.Api.Infrastructure.Persistence;
using BlinkShare.Api.Infrastructure.Security;
using DotNet.Testcontainers.Builders;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Testcontainers.PostgreSql;

namespace BlinkShare.Api.IntegrationTests.Infrastructure;

public sealed class BlinkShareApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private PostgreSqlContainer? _postgresContainer;
    private string _postgresConnectionString = string.Empty;

    public DateTimeOffset FixedUtcNow { get; } = new(2026, 4, 12, 10, 30, 0, TimeSpan.Zero);

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseContentRoot(TestPaths.GetApiContentRoot());

        builder.ConfigureAppConfiguration((_, configurationBuilder) =>
        {
            configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Postgres"] = _postgresConnectionString,
                ["ConnectionStrings:Redis"] = "localhost:6379,abortConnect=false",
                ["Shares:CreateText:MaxTextLength"] = "1000",
                ["Shares:CreateText:FreeTierTtlMinutes"] = "5",
                ["Shares:CreateFileUpload:AnonymousMaxFileSizeBytes"] = "524288",
                ["Shares:CreateFileUpload:FreeMaxFileSizeBytes"] = "1048576",
                ["Shares:CreateFileUpload:FreeTierTtlMinutes"] = "5",
                ["Shares:Unlock:LifetimeMinutes"] = "5"
            });
        });

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IClock>();
            services.AddSingleton<IClock>(new FixedClock(FixedUtcNow));

            services.RemoveAll<ICodeGenerator>();
            services.AddSingleton<ICodeGenerator>(new FixedCodeGenerator("TEST1234"));
            services.RemoveAll<IObjectStorage>();
            services.AddSingleton<IObjectStorage, FakeObjectStorage>();

            services.RemoveAll<IDbContextFactory<BlinkShareDbContext>>();
            services.RemoveAll<DbContextOptions<BlinkShareDbContext>>();
            services.AddPooledDbContextFactory<BlinkShareDbContext>(options =>
            {
                if (string.IsNullOrWhiteSpace(_postgresConnectionString))
                {
                    throw new InvalidOperationException("The PostgreSQL test container has not started.");
                }

                options.UseNpgsql(_postgresConnectionString, npgsql =>
                {
                    npgsql.EnableRetryOnFailure(3);
                    npgsql.CommandTimeout(5);
                });

                options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            });
        });
    }

    public async Task InitializeAsync()
    {
        _postgresContainer = new PostgreSqlBuilder("postgres:17-alpine")
            .WithDatabase("blinkshare_tests")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();

        await _postgresContainer.StartAsync();
        _postgresConnectionString = _postgresContainer.GetConnectionString();

        await using var scope = Services.CreateAsyncScope();
        var dbContextFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<BlinkShareDbContext>>();
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        await dbContext.Database.MigrateAsync();
    }

    public new async Task DisposeAsync()
    {
        Dispose();

        if (_postgresContainer is not null)
        {
            await _postgresContainer.DisposeAsync();
        }
    }

    public async Task ResetDatabaseAsync()
    {
        await using var scope = Services.CreateAsyncScope();
        var dbContextFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<BlinkShareDbContext>>();
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        await dbContext.AuthSessions.ExecuteDeleteAsync();
        await dbContext.Accounts.ExecuteDeleteAsync();
        await dbContext.Shares.ExecuteDeleteAsync();
    }

    private sealed class FixedClock(DateTimeOffset utcNow) : IClock
    {
        public DateTimeOffset UtcNow { get; } = utcNow;
    }

    private sealed class FixedCodeGenerator(string code) : ICodeGenerator
    {
        public string GenerateShareCode() => code;
    }

    private sealed class FakeObjectStorage : IObjectStorage
    {
        public Task<ObjectStorageUploadResult> CreateUploadTargetAsync(
            ObjectStorageUploadRequest request,
            CancellationToken cancellationToken) =>
            Task.FromResult(new ObjectStorageUploadResult(
                request.StorageKey,
                $"https://object-storage.test/upload/{Uri.EscapeDataString(request.StorageKey)}",
                "PUT"));

        public Task<ObjectStorageDownloadResult> CreateDownloadTargetAsync(
            ObjectStorageDownloadRequest request,
            CancellationToken cancellationToken) =>
            Task.FromResult(new ObjectStorageDownloadResult(
                request.StorageKey,
                $"https://object-storage.test/download/{Uri.EscapeDataString(request.StorageKey)}"));

        public Task DeleteObjectAsync(string storageKey, CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
