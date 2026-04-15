using BlinkShare.Api.Common.Time;
using BlinkShare.Api.Infrastructure.Persistence;
using BlinkShare.Worker.Features.CleanupObjects;
using BlinkShare.Worker.UnitTests.TestDoubles;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace BlinkShare.Worker.UnitTests.Features.CleanupObjects;

public sealed class HandlerTests
{
    [Fact]
    public async Task Expired_file_objects_are_deleted_and_marked_cleaned()
    {
        var dbContextFactory = CreateDbContextFactory(Guid.NewGuid().ToString("N"));
        var objectStorage = new FakeObjectStorage();

        await using (var dbContext = await dbContextFactory.CreateDbContextAsync())
        {
            await dbContext.Shares.AddAsync(new Share(
                Guid.NewGuid(),
                "FILE0001",
                ShareMode.StoredShare,
                ShareKind.File,
                ShareStatus.Expired,
                null,
                null,
                null,
                "report.txt",
                "text/plain",
                12,
                "shares/FILE0001/report.txt",
                new DateTimeOffset(2026, 4, 12, 10, 0, 0, TimeSpan.Zero),
                new DateTimeOffset(2026, 4, 12, 10, 29, 0, TimeSpan.Zero),
                null,
                0,
                null));
            await dbContext.SaveChangesAsync();
        }

        var handler = new Handler(
            dbContextFactory,
            objectStorage,
            new FakeClock(new DateTimeOffset(2026, 4, 12, 10, 30, 0, TimeSpan.Zero)),
            Options.Create(new BlinkShare.Worker.Features.CleanupObjects.WorkerOptions
            {
                BatchSize = 10
            }));

        var cleanedCount = await handler.HandleAsync(CancellationToken.None);

        Assert.Equal(1, cleanedCount);
        Assert.True(objectStorage.WasDeleted("shares/FILE0001/report.txt"));
    }

    private static IDbContextFactory<BlinkShareDbContext> CreateDbContextFactory(string databaseName)
    {
        var options = new DbContextOptionsBuilder<BlinkShareDbContext>()
            .UseSqlite(CreateOpenConnection(databaseName))
            .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
            .Options;

        var factory = new TestDbContextFactory(options);
        using var dbContext = factory.CreateDbContext();
        dbContext.Database.EnsureCreated();
        return factory;
    }

    private static SqliteConnection CreateOpenConnection(string databaseName)
    {
        var connection = new SqliteConnection($"Data Source={databaseName};Mode=Memory;Cache=Shared");
        connection.Open();
        return connection;
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
