namespace BlinkShare.Worker.Features.CleanupObjects;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCleanupObjectsFeature(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddOptions<WorkerOptions>()
            .Bind(configuration.GetSection("Worker:CleanupObjects"));

        services.AddTransient<Handler>();
        services.AddHostedService<BackgroundWorker>();

        return services;
    }
}
