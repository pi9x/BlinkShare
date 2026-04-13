using System.Globalization;
using Microsoft.AspNetCore.DataProtection;

namespace BlinkShare.Api.Infrastructure.Security;

public sealed class DataProtectionShareUnlockProofService(IDataProtectionProvider dataProtectionProvider)
    : IShareUnlockProofService
{
    private readonly IDataProtector _protector = dataProtectionProvider.CreateProtector("blinkshare.share-unlock-proof.v1");

    public string CreateProof(Guid shareId, string code, DateTimeOffset unlockedUntilUtc)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);

        var payload = string.Join(
            '|',
            shareId.ToString("D"),
            code,
            unlockedUntilUtc.UtcTicks.ToString(CultureInfo.InvariantCulture));

        return _protector.Protect(payload);
    }

    public bool TryValidate(
        string proof,
        Guid expectedShareId,
        string expectedCode,
        DateTimeOffset now,
        out DateTimeOffset unlockedUntilUtc)
    {
        unlockedUntilUtc = default;

        if (string.IsNullOrWhiteSpace(proof))
        {
            return false;
        }

        try
        {
            var payload = _protector.Unprotect(proof);
            var segments = payload.Split('|', StringSplitOptions.None);

            if (segments.Length != 3 ||
                !Guid.TryParse(segments[0], out var shareId) ||
                !long.TryParse(segments[2], out var utcTicks))
            {
                return false;
            }

            if (shareId != expectedShareId || !string.Equals(segments[1], expectedCode, StringComparison.Ordinal))
            {
                return false;
            }

            unlockedUntilUtc = new DateTimeOffset(utcTicks, TimeSpan.Zero);

            return unlockedUntilUtc > now;
        }
        catch
        {
            return false;
        }
    }
}
