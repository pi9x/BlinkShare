using Microsoft.Extensions.Options;

namespace BlinkShare.Api.Features.Shares.CreateText;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCreateTextFeature(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddOptions<CreateTextOptions>()
            .Bind(configuration.GetSection(CreateTextOptions.SectionName))
            .Validate(options => options.MaxTextLength > 0, "MaxTextLength must be greater than zero.")
            .Validate(options => options.FreeTierTtlMinutes > 0, "FreeTierTtlMinutes must be greater than zero.")
            .ValidateOnStart();

        services.AddTransient<Validator>();
        services.AddTransient<Handler>();

        return services;
    }
}
