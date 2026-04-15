using BlinkShare.Api.Common.Time;
using BlinkShare.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BlinkShare.Worker.Features.ExpireShares;

public sealed class Handler(
    IDbContextFactory<BlinkShareDbContext> dbContextFactory,
    IClock clock)
{
    public async Task<int> HandleAsync(CancellationToken cancellationToken)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        return await dbContext.Shares
            .Where(share => share.ExpiresAtUtc != null && share.ExpiresAtUtc <= clock.UtcNow)
            .Where(share => share.Status != ShareStatus.Expired && share.Status != ShareStatus.Deleted)
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(share => share.Status, ShareStatus.Expired),
                cancellationToken);
    }
}
