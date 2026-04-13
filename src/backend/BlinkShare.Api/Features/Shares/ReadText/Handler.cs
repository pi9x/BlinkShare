using BlinkShare.Api.Common.Results;
using BlinkShare.Api.Common.Time;
using BlinkShare.Api.Infrastructure.Persistence;
using BlinkShare.Api.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;

namespace BlinkShare.Api.Features.Shares.ReadText;

public sealed class Handler(
    IDbContextFactory<BlinkShareDbContext> dbContextFactory,
    IClock clock,
    IShareUnlockProofService shareUnlockProofService) : BlinkShare.Api.Common.DependencyInjection.ISliceService
{
    public async Task<Result<Response>> HandleAsync(
        string code,
        string? unlockProof,
        CancellationToken cancellationToken)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        var share = await dbContext.Shares
            .AsNoTracking()
            .Where(candidate => candidate.Code == code)
            .Select(candidate => new ShareReadModel(
                candidate.Id,
                candidate.Code,
                candidate.Kind,
                candidate.ExpiresAtUtc,
                candidate.PasscodeHash != null,
                candidate.TextInline))
            .SingleOrDefaultAsync(cancellationToken);

        if (share is null)
        {
            return Result<Response>.Failure(Errors.Share.NotFound());
        }

        if (share.ExpiresAtUtc is { } expiresAtUtc && expiresAtUtc <= clock.UtcNow)
        {
            return Result<Response>.Failure(Errors.Share.Expired());
        }

        if (share.Kind != ShareKind.Text)
        {
            return Result<Response>.Failure(Errors.Share.InvalidKind("Only text shares can be read from this endpoint."));
        }

        if (share.HasPasscode &&
            !shareUnlockProofService.TryValidate(unlockProof ?? string.Empty, share.Id, share.Code, clock.UtcNow, out _))
        {
            return Result<Response>.Failure(Errors.Share.PasscodeRequired());
        }

        var trackedShare = await dbContext.Shares.SingleAsync(candidate => candidate.Id == share.Id, cancellationToken);
        trackedShare.MarkAccessed(clock.UtcNow);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result<Response>.Success(new Response(
            share.Id,
            share.Code,
            share.Text ?? string.Empty,
            share.ExpiresAtUtc));
    }

    private sealed record ShareReadModel(
        Guid Id,
        string Code,
        ShareKind Kind,
        DateTimeOffset? ExpiresAtUtc,
        bool HasPasscode,
        string? Text);
}
