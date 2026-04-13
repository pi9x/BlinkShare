using BlinkShare.Api.Infrastructure.Security;

namespace BlinkShare.Api.UnitTests.Infrastructure.Security;

public sealed class Pbkdf2PasscodeHasherTests
{
    [Fact]
    public void Verify_returns_true_for_matching_passcode()
    {
        var hasher = new Pbkdf2PasscodeHasher();
        var hash = hasher.Hash("secret");

        var result = hasher.Verify("secret", hash);

        Assert.True(result);
    }

    [Fact]
    public void Verify_returns_false_for_non_matching_passcode()
    {
        var hasher = new Pbkdf2PasscodeHasher();
        var hash = hasher.Hash("secret");

        var result = hasher.Verify("wrong", hash);

        Assert.False(result);
    }
}
