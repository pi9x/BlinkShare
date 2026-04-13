using BlinkShare.Api.Common.Results;
using BlinkShare.Api.Common.Time;
using BlinkShare.Api.Infrastructure.ObjectStorage;
using BlinkShare.Api.Infrastructure.Persistence;
using BlinkShare.Api.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;

namespace BlinkShare.Api.Features.Shares.RequestDownload;

public sealed class Handler(
    IDbContextFactory<BlinkShareDbContext> dbContextFactory,
    IObjectStorage objectStorage,
    IShareUnlockProofService shareUnlockProofService,
    IClock clock) : BlinkShare.Api.Common.DependencyInjection.ISliceService
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
            .Select(candidate => new ShareDownloadModel(
                candidate.Id,
                candidate.Code,
                candidate.Kind,
                candidate.Status,
                candidate.ExpiresAtUtc,
                candidate.PasscodeHash != null,
                candidate.StorageKey,
                candidate.FileName,
                candidate.ContentType,
                candidate.DownloadCount,
                candidate.MaxDownloadCount))
            .SingleOrDefaultAsync(cancellationToken);

        if (share is null)
        {
            return Result<Response>.Failure(Errors.Share.NotFound());
        }

        if (share.ExpiresAtUtc is { } expiresAtUtc && expiresAtUtc <= clock.UtcNow)
        {
            return Result<Response>.Failure(Errors.Share.Expired());
        }

        if (share.Kind != ShareKind.File)
        {
            return Result<Response>.Failure(Errors.Share.InvalidKind("Only file shares can be downloaded from this endpoint."));
        }

        if (share.Status != ShareStatus.Ready)
        {
            return Result<Response>.Failure(Errors.Share.NotReady("Only ready file shares can be downloaded."));
        }

        if (share.MaxDownloadCount is { } maxDownloadCount && share.DownloadCount >= maxDownloadCount)
        {
            return Result<Response>.Failure(Errors.Share.MaxDownloadsReached());
        }

        if (share.HasPasscode &&
            !shareUnlockProofService.TryValidate(unlockProof ?? string.Empty, share.Id, share.Code, clock.UtcNow, out _))
        {
            return Result<Response>.Failure(Errors.Share.PasscodeRequired());
        }

        if (string.IsNullOrWhiteSpace(share.StorageKey))
        {
            return Result<Response>.Failure(Errors.Share.InvalidStatus("File share is missing a storage key."));
        }

        var downloadTarget = await objectStorage.CreateDownloadTargetAsync(
            new ObjectStorageDownloadRequest(
                share.StorageKey,
                share.FileName,
                share.ContentType,
                clock.UtcNow.AddMinutes(5)),
            cancellationToken);

        var trackedShare = await dbContext.Shares.SingleAsync(candidate => candidate.Id == share.Id, cancellationToken);
        trackedShare.RecordDownload(clock.UtcNow);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result<Response>.Success(new Response(
            trackedShare.Id,
            trackedShare.Code,
            downloadTarget.DownloadUrl,
            trackedShare.DownloadCount));
    }

    private sealed record ShareDownloadModel(
        Guid Id,
        string Code,
        ShareKind Kind,
        ShareStatus Status,
        DateTimeOffset? ExpiresAtUtc,
        bool HasPasscode,
        string? StorageKey,
        string? FileName,
        string? ContentType,
        int DownloadCount,
        int? MaxDownloadCount);
}
