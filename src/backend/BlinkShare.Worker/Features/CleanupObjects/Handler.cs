using BlinkShare.Api.Common.Time;
using BlinkShare.Api.Infrastructure.ObjectStorage;
using BlinkShare.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace BlinkShare.Worker.Features.CleanupObjects;

public sealed class Handler(
    IDbContextFactory<BlinkShareDbContext> dbContextFactory,
    IObjectStorage objectStorage,
    IClock clock,
    IOptions<WorkerOptions> options)
{
    public async Task<int> HandleAsync(CancellationToken cancellationToken)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        var candidates = await dbContext.Shares
            .Where(share => share.Kind == ShareKind.File)
            .Where(share => share.StorageKey != null)
            .Where(share => share.StorageCleanupCompletedAtUtc == null)
            .Where(share => share.Status == ShareStatus.Expired || share.Status == ShareStatus.Deleted)
            .OrderBy(share => share.ExpiresAtUtc)
            .Take(options.Value.BatchSize)
            .ToListAsync(cancellationToken);

        foreach (var share in candidates)
        {
            await objectStorage.DeleteObjectAsync(share.StorageKey!, cancellationToken);
            share.MarkStorageCleanupCompleted(clock.UtcNow);
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return candidates.Count;
    }
}
