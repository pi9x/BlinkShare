using BlinkShare.Api.Common.Results;
using BlinkShare.Api.Common.Time;
using BlinkShare.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BlinkShare.Api.Features.Shares.CompleteFileUpload;

public sealed class Handler(
    IDbContextFactory<BlinkShareDbContext> dbContextFactory,
    IClock clock) : BlinkShare.Api.Common.DependencyInjection.ISliceService
{
    public async Task<Result<Response>> HandleAsync(string code, CancellationToken cancellationToken)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        var share = await dbContext.Shares.SingleOrDefaultAsync(candidate => candidate.Code == code, cancellationToken);

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
            return Result<Response>.Failure(Errors.Share.InvalidKind("Only file shares can be completed from this endpoint."));
        }

        if (share.Status != ShareStatus.Pending)
        {
            return Result<Response>.Failure(Errors.Share.InvalidStatus("Only pending file shares can transition to ready."));
        }

        await dbContext.Shares
            .Where(candidate => candidate.Id == share.Id)
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(candidate => candidate.Status, ShareStatus.Ready),
                cancellationToken);

        return Result<Response>.Success(new Response(share.Id, share.Code, ShareStatus.Ready));
    }
}
