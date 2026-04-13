using System.Reflection;
using BlinkShare.Api.Common.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BlinkShare.Api.Common.Endpoints;

public static class EndpointRegistrationExtensions
{
    public static IServiceCollection AddBlinkShareEndpoints(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        var endpointTypes = GetEndpointTypes();
        var featureServiceTypes = GetFeatureServiceTypes();

        foreach (var endpointType in endpointTypes)
        {
            services.TryAddEnumerable(ServiceDescriptor.Singleton(typeof(IEndpoint), endpointType));
        }

        foreach (var featureServiceType in featureServiceTypes)
        {
            services.TryAdd(ServiceDescriptor.Transient(featureServiceType, featureServiceType));
        }

        return services;
    }

    public static IEndpointRouteBuilder MapBlinkShareEndpoints(this IEndpointRouteBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        foreach (var endpoint in app.ServiceProvider.GetRequiredService<IEnumerable<IEndpoint>>())
        {
            endpoint.MapEndpoint(app);
        }

        return app;
    }

    private static IReadOnlyList<Type> GetEndpointTypes() =>
        typeof(Program).Assembly
            .DefinedTypes
            .Where(type => type is { IsAbstract: false, IsInterface: false })
            .Where(type => typeof(IEndpoint).IsAssignableFrom(type))
            .Select(type => type.AsType())
            .OrderBy(type => type.FullName, StringComparer.Ordinal)
            .ToArray();

    private static IReadOnlyList<Type> GetFeatureServiceTypes() =>
        typeof(Program).Assembly
            .DefinedTypes
            .Where(type => type is { IsAbstract: false, IsInterface: false })
            .Where(type => typeof(ISliceService).IsAssignableFrom(type))
            .Select(type => type.AsType())
            .OrderBy(type => type.FullName, StringComparer.Ordinal)
            .ToArray();
}
