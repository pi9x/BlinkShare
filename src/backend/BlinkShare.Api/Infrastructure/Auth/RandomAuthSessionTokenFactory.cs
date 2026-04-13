using System.Security.Cryptography;

namespace BlinkShare.Api.Infrastructure.Auth;

public sealed class RandomAuthSessionTokenFactory : IAuthSessionTokenFactory
{
    public string CreateToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }
}
