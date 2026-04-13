using BlinkShare.Api.Common.Time;
using BlinkShare.Api.Features.Shares.CompleteFileUpload;
using BlinkShare.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BlinkShare.Api.UnitTests.Features.Shares.CompleteFileUpload;

public sealed class HandlerTests
{
    [Fact]
    public async Task Non_pending_share_returns_invalid_status()
    {
        var databaseName = Guid.NewGuid().ToString("N");
        var dbContextFactory = CreateDbContextFactory(databaseName);

        await SeedShareAsync(
            dbContextFactory,
            new Share(
                Guid.NewGuid(),
                "FILE1234",
                ShareTier.Free,
                ShareMode.StoredShare,
                ShareKind.File,
                ShareStatus.Ready,
                null,
                null,
                null,
                "report.txt",
                "text/plain",
                12,
                "shares/FILE1234/report.txt",
                new DateTimeOffset(2026, 4, 12, 10, 0, 0, TimeSpan.Zero),
                new DateTimeOffset(2026, 4, 12, 10, 35, 0, TimeSpan.Zero),
                null,
                0,
                null));

        var handler = new Handler(dbContextFactory, new FakeClock(new DateTimeOffset(2026, 4, 12, 10, 30, 0, TimeSpan.Zero)));

        var result = await handler.HandleAsync("FILE1234", CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("share.invalid_status", result.Error!.Code);
    }

    private static IDbContextFactory<BlinkShareDbContext> CreateDbContextFactory(string databaseName)
    {
        var options = new DbContextOptionsBuilder<BlinkShareDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;

        return new TestDbContextFactory(options);
    }

    private static async Task SeedShareAsync(IDbContextFactory<BlinkShareDbContext> dbContextFactory, Share share)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        await dbContext.Shares.AddAsync(share);
        await dbContext.SaveChangesAsync();
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
