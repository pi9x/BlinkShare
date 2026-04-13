using DotNet.Testcontainers.Builders;
using Testcontainers.PostgreSql;

namespace BlinkShare.Api.IntegrationTests.Infrastructure;

[AttributeUsage(AttributeTargets.Method)]
public sealed class RequiresDockerFactAttribute : FactAttribute
{
    private static readonly Lazy<string?> SkipReason = new(ResolveSkipReason);

    public RequiresDockerFactAttribute()
    {
        Skip = SkipReason.Value;
    }

    private static string? ResolveSkipReason()
    {
        try
        {
            _ = new PostgreSqlBuilder("postgres:17-alpine")
                .WithDatabase("blinkshare_tests")
                .WithUsername("postgres")
                .WithPassword("postgres")
                .Build();

            return null;
        }
        catch (DockerUnavailableException exception)
        {
            return $"Requires Docker or Podman to run PostgreSQL-backed integration tests: {exception.Message}";
        }
    }
}
