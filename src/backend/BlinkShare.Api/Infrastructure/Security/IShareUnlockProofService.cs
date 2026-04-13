namespace BlinkShare.Api.Infrastructure.Security;

public interface IShareUnlockProofService
{
    string CreateProof(Guid shareId, string code, DateTimeOffset unlockedUntilUtc);

    bool TryValidate(
        string proof,
        Guid expectedShareId,
        string expectedCode,
        DateTimeOffset now,
        out DateTimeOffset unlockedUntilUtc);
}
