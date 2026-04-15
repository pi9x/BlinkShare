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
            .OrderBy(share => share.Id)
            .Take(options.Value.BatchSize)
            .Select(share => new CleanupCandidate(share.Id, share.StorageKey!))
            .ToListAsync(cancellationToken);

        foreach (var candidate in candidates)
        {
            await objectStorage.DeleteObjectAsync(candidate.StorageKey, cancellationToken);
        }

        await MarkCleanupCompletedAsync(
            dbContext,
            candidates.Select(candidate => candidate.Id).ToArray(),
            clock.UtcNow,
            cancellationToken);

        return candidates.Count;
    }

    private static async Task MarkCleanupCompletedAsync(
        BlinkShareDbContext dbContext,
        Guid[] shareIds,
        DateTimeOffset completedAtUtc,
        CancellationToken cancellationToken)
    {
        if (shareIds.Length == 0)
        {
            return;
        }

        await dbContext.Shares
            .Where(share => shareIds.Contains(share.Id))
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(share => share.StorageCleanupCompletedAtUtc, completedAtUtc),
                cancellationToken);
    }

    private sealed record CleanupCandidate(Guid Id, string StorageKey);
}
