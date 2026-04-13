namespace BlinkShare.Api.Features.AnonymousSessions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAnonymousSessionFeatures(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddOptions<AnonymousSessionOptions>()
            .Bind(configuration.GetSection("AnonymousSessions"))
            .Validate(options => options.SessionLifetimeMinutes > 0, "SessionLifetimeMinutes must be positive.")
            .Validate(options => options.ReconnectGraceSeconds > 0, "ReconnectGraceSeconds must be positive.")
            .Validate(options => options.MaxTextLength > 0, "MaxTextLength must be positive.")
            .Validate(options => options.MaxFileSizeBytes > 0, "MaxFileSizeBytes must be positive.");

        services.AddSingleton<CreateOrJoin.Validator>();
        services.AddSingleton<PublishText.Validator>();
        services.AddSingleton<PublishFileMetadata.Validator>();
        services.AddSingleton<PeerSessionAccessService>();

        return services;
    }
}
