using BlinkShare.Api.Infrastructure.Auth;
using BlinkShare.Api.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication;

namespace BlinkShare.Api.Common.Auth;

public static class AuthServiceCollectionExtensions
{
    public static IServiceCollection AddBlinkShareAuth(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddOptions<AuthSessionOptions>()
            .Bind(configuration.GetSection("Auth"))
            .Validate(options => options.SessionLifetimeHours > 0, "SessionLifetimeHours must be positive.");

        services.AddHttpContextAccessor();
        services.AddSingleton<ICurrentAccountAccessor, HttpContextCurrentAccountAccessor>();
        services.AddSingleton<IAccountPasswordHasher, Pbkdf2AccountPasswordHasher>();
        services.AddSingleton<IAuthSessionTokenFactory, RandomAuthSessionTokenFactory>();

        services.AddAuthentication(BlinkShareAuthConstants.SchemeName)
            .AddScheme<AuthenticationSchemeOptions, BlinkShareBearerAuthenticationHandler>(
                BlinkShareAuthConstants.SchemeName,
                _ => { });

        services.AddAuthorization();

        return services;
    }
}
