using BlinkShare.Api.Infrastructure.Auth;

namespace BlinkShare.Api.UnitTests.Infrastructure.Auth;

public sealed class RandomAuthSessionTokenFactoryTests
{
    [Fact]
    public void Create_token_returns_url_safe_value()
    {
        var factory = new RandomAuthSessionTokenFactory();

        var token = factory.CreateToken();

        Assert.False(string.IsNullOrWhiteSpace(token));
        Assert.DoesNotContain("+", token, StringComparison.Ordinal);
        Assert.DoesNotContain("/", token, StringComparison.Ordinal);
        Assert.DoesNotContain("=", token, StringComparison.Ordinal);
    }
}
