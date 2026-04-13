using BlinkShare.Api.Common.Time;
using BlinkShare.Api.Infrastructure.ObjectStorage;
using BlinkShare.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BlinkShare.Worker;

public static class WorkerInfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddBlinkShareWorkerInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var postgresConnectionString = configuration.GetConnectionString("Postgres")
            ?? throw new InvalidOperationException("Connection string 'Postgres' was not found.");

        services.AddPooledDbContextFactory<BlinkShareDbContext>(options =>
        {
            options.UseNpgsql(postgresConnectionString, npgsql =>
            {
                npgsql.EnableRetryOnFailure(3);
                npgsql.CommandTimeout(5);
            });

            options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        });

        services.AddOptions<ObjectStorageOptions>()
            .Bind(configuration.GetSection("ObjectStorage"));

        services.AddSingleton<IClock, SystemClock>();
        services.AddSingleton<IObjectStorage, DevelopmentObjectStorage>();

        return services;
    }
}
