using BlinkShare.Api.Common.Endpoints;
using Microsoft.Extensions.DependencyInjection;

namespace BlinkShare.Api.UnitTests.Common.Endpoints;

public sealed class EndpointRegistrationExtensionsTests
{
    [Fact]
    public void AddBlinkShareEndpoints_registers_health_endpoint()
    {
        var services = new ServiceCollection();

        services.AddBlinkShareEndpoints();
        
        Assert.Contains(services, service => service.ServiceType == typeof(IEndpoint) &&
            service.ImplementationType == typeof(BlinkShare.Api.Features.Health.Endpoint));
    }

    [Fact]
    public void AddBlinkShareEndpoints_registers_slice_services_via_marker_interface()
    {
        var services = new ServiceCollection();

        services.AddBlinkShareEndpoints();

        Assert.Contains(services, service => service.ServiceType == typeof(BlinkShare.Api.Features.Shares.GetByCode.Handler));
        Assert.DoesNotContain(services, service => service.ServiceType == typeof(BlinkShare.Api.Features.Shares.CreateText.Request));
    }
}
