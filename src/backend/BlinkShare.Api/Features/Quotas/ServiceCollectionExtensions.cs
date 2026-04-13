namespace BlinkShare.Api.Features.Quotas;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddQuotaFeature(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddOptions<QuotaOptions>()
            .Bind(configuration.GetSection("Quotas"));

        return services;
    }
}
