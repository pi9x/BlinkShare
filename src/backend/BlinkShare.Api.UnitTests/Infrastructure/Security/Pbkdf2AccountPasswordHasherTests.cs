using BlinkShare.Api.Infrastructure.Security;

namespace BlinkShare.Api.UnitTests.Infrastructure.Security;

public sealed class Pbkdf2AccountPasswordHasherTests
{
    [Fact]
    public void Empty_password_hashes_and_verifies_successfully()
    {
        var hasher = new Pbkdf2AccountPasswordHasher();
        var hash = hasher.Hash(string.Empty);

        var result = hasher.Verify(string.Empty, hash);

        Assert.True(result);
    }

    [Fact]
    public void Wrong_password_verification_fails()
    {
        var hasher = new Pbkdf2AccountPasswordHasher();
        var hash = hasher.Hash("secret");

        var result = hasher.Verify("wrong", hash);

        Assert.False(result);
    }
}
