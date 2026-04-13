using System.Security.Cryptography;
using System.Text;

namespace BlinkShare.Api.Infrastructure.Auth;

public static class AuthSessionTokenHasher
{
    public static string Hash(string token)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(token);
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
    }
}
