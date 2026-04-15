using BlinkShare.Api.Common.Results;
using BlinkShare.Api.Common.Time;
using BlinkShare.Api.Infrastructure.Persistence;
using BlinkShare.Api.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace BlinkShare.Api.Features.Shares.Unlock;

public sealed class Handler(
    IDbContextFactory<BlinkShareDbContext> dbContextFactory,
    IPasscodeHasher passcodeHasher,
    IShareUnlockProofService shareUnlockProofService,
    IClock clock,
    IOptions<ShareUnlockProofOptions> options) : BlinkShare.Api.Common.DependencyInjection.ISliceService
{
    public async Task<Result<Response>> HandleAsync(
        string code,
        string? passcode,
        CancellationToken cancellationToken)
    {
        if (passcode is null)
        {
            return Result<Response>.Failure(Errors.General.Validation("Passcode is required."));
        }

        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        var share = await dbContext.Shares
            .Where(candidate => candidate.Code == code)
            .Select(candidate => new ShareUnlockModel(candidate.Id, candidate.Code, candidate.ExpiresAtUtc, candidate.PasscodeHash))
            .SingleOrDefaultAsync(cancellationToken);

        if (share is null)
        {
            return Result<Response>.Failure(Errors.Share.NotFound());
        }

        if (share.ExpiresAtUtc is { } expiresAtUtc && expiresAtUtc <= clock.UtcNow)
        {
            return Result<Response>.Failure(Errors.Share.Expired());
        }

        if (string.IsNullOrWhiteSpace(share.PasscodeHash))
        {
            return Result<Response>.Failure(Errors.Share.PasscodeNotRequired());
        }

        if (!passcodeHasher.Verify(passcode, share.PasscodeHash))
        {
            return Result<Response>.Failure(Errors.Share.InvalidPasscode());
        }

        var unlockedUntilUtc = clock.UtcNow.AddMinutes(options.Value.LifetimeMinutes);
        var unlockProof = shareUnlockProofService.CreateProof(share.Id, share.Code, unlockedUntilUtc);

        return Result<Response>.Success(new Response(share.Id, share.Code, unlockProof, unlockedUntilUtc));
    }

    private sealed record ShareUnlockModel(
        Guid Id,
        string Code,
        DateTimeOffset? ExpiresAtUtc,
        string? PasscodeHash);
}
