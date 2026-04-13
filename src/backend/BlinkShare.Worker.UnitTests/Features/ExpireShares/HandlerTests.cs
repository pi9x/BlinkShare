using BlinkShare.Api.Common.Time;
using BlinkShare.Api.Infrastructure.Persistence;
using BlinkShare.Worker.Features.ExpireShares;
using Microsoft.EntityFrameworkCore;

namespace BlinkShare.Worker.UnitTests.Features.ExpireShares;

public sealed class HandlerTests
{
    [Fact]
    public async Task Eligible_shares_are_marked_expired()
    {
        var dbContextFactory = CreateDbContextFactory(Guid.NewGuid().ToString("N"));

        await using (var dbContext = await dbContextFactory.CreateDbContextAsync())
        {
            await dbContext.Shares.AddAsync(new Share(
                Guid.NewGuid(),
                "EXPIRE01",
                ShareTier.Free,
                ShareMode.StoredShare,
                ShareKind.Text,
                ShareStatus.Ready,
                null,
                null,
                "hello",
                null,
                "text/plain; charset=utf-8",
                5,
                null,
                new DateTimeOffset(2026, 4, 12, 10, 0, 0, TimeSpan.Zero),
                new DateTimeOffset(2026, 4, 12, 10, 29, 0, TimeSpan.Zero),
                null,
                0,
                null));
            await dbContext.SaveChangesAsync();
        }

        var handler = new Handler(dbContextFactory, new FakeClock(new DateTimeOffset(2026, 4, 12, 10, 30, 0, TimeSpan.Zero)));

        var updatedCount = await handler.HandleAsync(CancellationToken.None);

        Assert.Equal(1, updatedCount);
    }

    private static IDbContextFactory<BlinkShareDbContext> CreateDbContextFactory(string databaseName)
    {
        var options = new DbContextOptionsBuilder<BlinkShareDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;

        return new TestDbContextFactory(options);
    }

    private sealed class TestDbContextFactory(DbContextOptions<BlinkShareDbContext> options)
        : IDbContextFactory<BlinkShareDbContext>
    {
        public BlinkShareDbContext CreateDbContext() => new(options);

        public Task<BlinkShareDbContext> CreateDbContextAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(CreateDbContext());
    }

    private sealed class FakeClock(DateTimeOffset utcNow) : IClock
    {
        public DateTimeOffset UtcNow { get; } = utcNow;
    }
}
