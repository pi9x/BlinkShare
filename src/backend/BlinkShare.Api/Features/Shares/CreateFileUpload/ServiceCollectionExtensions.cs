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
            .Validate(options => options.AnonymousMaxFileSizeBytes > 0, "AnonymousMaxFileSizeBytes must be positive.")
            .Validate(options => options.FreeMaxFileSizeBytes > 0, "FreeMaxFileSizeBytes must be positive.")
            .Validate(options => options.FreeTierTtlMinutes > 0, "FreeTierTtlMinutes must be positive.");

        return services;
    }
}
