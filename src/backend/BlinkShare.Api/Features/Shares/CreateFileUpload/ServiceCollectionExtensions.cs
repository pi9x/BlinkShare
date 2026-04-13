namespace BlinkShare.Api.Features.Shares.CreateFileUpload;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCreateFileUploadFeature(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddOptions<CreateFileUploadOptions>()
            .Bind(configuration.GetSection("Shares:CreateFileUpload"))
            .Validate(options => options.MaxFileSizeBytes > 0, "MaxFileSizeBytes must be positive.")
            .Validate(options => options.FreeTierTtlMinutes > 0, "FreeTierTtlMinutes must be positive.");

        services.AddSingleton<Validator>();

        return services;
    }
}
