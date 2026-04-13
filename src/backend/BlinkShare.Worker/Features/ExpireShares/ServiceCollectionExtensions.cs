namespace BlinkShare.Worker.Features.ExpireShares;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddExpireSharesFeature(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddOptions<WorkerOptions>()
            .Bind(configuration.GetSection("Worker:ExpireShares"));

        services.AddTransient<Handler>();
        services.AddHostedService<BackgroundWorker>();

        return services;
    }
}
