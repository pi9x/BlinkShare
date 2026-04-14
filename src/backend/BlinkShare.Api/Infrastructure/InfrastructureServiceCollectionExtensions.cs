using BlinkShare.Api.Common.Time;
using BlinkShare.Api.Infrastructure.ObjectStorage;
using BlinkShare.Api.Infrastructure.Persistence;
using BlinkShare.Api.Infrastructure.Redis;
using BlinkShare.Api.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace BlinkShare.Api.Infrastructure;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddBlinkShareInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var postgresConnectionString = configuration.GetConnectionString("Postgres")
            ?? throw new InvalidOperationException("Connection string 'Postgres' was not found.");

        var redisConnectionString = configuration.GetConnectionString("Redis")
            ?? throw new InvalidOperationException("Connection string 'Redis' was not found.");

        services.AddOpenApi();
        services.AddDataProtection();
        services.AddSignalR()
            .AddStackExchangeRedis(redisConnectionString);

        services.AddPooledDbContextFactory<BlinkShareDbContext>(options =>
        {
            options.UseNpgsql(postgresConnectionString, npgsql =>
            {
                npgsql.EnableRetryOnFailure(3);
                npgsql.CommandTimeout(5);
            });

            options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        });

        services.AddHealthChecks()
            .AddNpgSql(postgresConnectionString, name: "postgres")
            .AddRedis(redisConnectionString, name: "redis");

        services.AddSingleton<ConfigurationOptions>(_ =>
        {
            var redisOptions = ConfigurationOptions.Parse(redisConnectionString, ignoreUnknown: true);
            redisOptions.AbortOnConnectFail = false;
            return redisOptions;
        });

        services.AddSingleton<IConnectionMultiplexer>(serviceProvider =>
            ConnectionMultiplexer.Connect(serviceProvider.GetRequiredService<ConfigurationOptions>()));

        services.AddOptions<ObjectStorageOptions>()
            .Bind(configuration.GetSection("ObjectStorage"))
            .ValidateDataAnnotations()
            .Validate(static options => !string.IsNullOrWhiteSpace(options.BucketName), "ObjectStorage:BucketName is required.")
            .ValidateOnStart();

        services.AddOptions<ShareUnlockProofOptions>()
            .Bind(configuration.GetSection("Shares:Unlock"));

        services.AddSingleton<IClock, SystemClock>();
        services.AddSingleton<ICodeGenerator, RandomCodeGenerator>();
        services.AddSingleton<IPasscodeHasher, Pbkdf2PasscodeHasher>();
        services.AddSingleton<IShareUnlockProofService, DataProtectionShareUnlockProofService>();
        services.AddSingleton<IObjectStorage, S3CompatibleObjectStorage>();
        services.AddSingleton<IPeerSessionStore, RedisPeerSessionStore>();

        return services;
    }
}
