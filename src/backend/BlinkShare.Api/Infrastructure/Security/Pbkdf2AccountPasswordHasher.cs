using System.Security.Cryptography;

namespace BlinkShare.Api.Infrastructure.Security;

public sealed class Pbkdf2AccountPasswordHasher : IAccountPasswordHasher
{
    private const string Algorithm = "pbkdf2-sha256";
    private const int Iterations = 210_000;
    private const int SaltSize = 16;
    private const int KeySize = 32;

    public string Hash(string password)
    {
        ArgumentNullException.ThrowIfNull(password);

        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var key = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, KeySize);

        return string.Join(
            '$',
            Algorithm,
            Iterations.ToString(),
            Convert.ToBase64String(salt),
            Convert.ToBase64String(key));
    }

    public bool Verify(string password, string persistedHash)
    {
        ArgumentNullException.ThrowIfNull(password);

        if (string.IsNullOrWhiteSpace(persistedHash))
        {
            return false;
        }

        var segments = persistedHash.Split('$', StringSplitOptions.None);
        if (segments.Length != 4 || !string.Equals(segments[0], Algorithm, StringComparison.Ordinal))
        {
            return false;
        }

        if (!int.TryParse(segments[1], out var iterations))
        {
            return false;
        }

        try
        {
            var salt = Convert.FromBase64String(segments[2]);
            var expectedKey = Convert.FromBase64String(segments[3]);
            var actualKey = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, expectedKey.Length);

            return CryptographicOperations.FixedTimeEquals(actualKey, expectedKey);
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
