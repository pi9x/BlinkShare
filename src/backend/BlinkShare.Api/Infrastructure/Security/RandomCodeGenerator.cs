using System.Security.Cryptography;

namespace BlinkShare.Api.Infrastructure.Security;

public sealed class RandomCodeGenerator : ICodeGenerator
{
    private const string Alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
    private const int CodeLength = 8;

    public string GenerateShareCode()
    {
        var buffer = new char[CodeLength];

        for (var index = 0; index < buffer.Length; index++)
        {
            buffer[index] = Alphabet[RandomNumberGenerator.GetInt32(Alphabet.Length)];
        }

        return new string(buffer);
    }
}
