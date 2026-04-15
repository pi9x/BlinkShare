using BlinkShare.Api.Common.Time;
using BlinkShare.Api.Features.Shares.RequestDownload;
using BlinkShare.Api.Infrastructure.ObjectStorage;
using BlinkShare.Api.Infrastructure.Persistence;
using BlinkShare.Api.Infrastructure.Security;
using BlinkShare.Api.UnitTests.TestDoubles;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace BlinkShare.Api.UnitTests.Features.Shares.RequestDownload;

public sealed class HandlerTests
{
    [Fact]
    public async Task Text_share_returns_invalid_kind()
    {
        var dbContextFactory = CreateDbContextFactory(Guid.NewGuid().ToString("N"));

        await SeedShareAsync(dbContextFactory, CreateTextShare("TEXT0001"));

        var handler = CreateHandler(dbContextFactory);

        var result = await handler.HandleAsync("TEXT0001", null, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("share.invalid_kind", result.Error!.Code);
    }

    [Fact]
    public async Task Pending_file_returns_not_ready()
    {
        var dbContextFactory = CreateDbContextFactory(Guid.NewGuid().ToString("N"));

        await SeedShareAsync(dbContextFactory, CreateFileShare("FILE0001", ShareStatus.Pending, 0, null));

        var handler = CreateHandler(dbContextFactory);

        var result = await handler.HandleAsync("FILE0001", null, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("share.not_ready", result.Error!.Code);
    }

    [Fact]
    public async Task Max_download_limit_returns_failure()
    {
        var dbContextFactory = CreateDbContextFactory(Guid.NewGuid().ToString("N"));

        await SeedShareAsync(dbContextFactory, CreateFileShare("FILE0001", ShareStatus.Ready, 2, 2));

        var handler = CreateHandler(dbContextFactory);

        var result = await handler.HandleAsync("FILE0001", null, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("share.max_downloads_reached", result.Error!.Code);
    }

    private static Handler CreateHandler(IDbContextFactory<BlinkShareDbContext> dbContextFactory) =>
        new(
            dbContextFactory,
            new FakeObjectStorage(),
            new FakeShareUnlockProofService(),
            new FakeClock(new DateTimeOffset(2026, 4, 12, 10, 30, 0, TimeSpan.Zero)));

    private static Share CreateTextShare(string code) =>
        new(
            Guid.NewGuid(),
            code,
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
            new DateTimeOffset(2026, 4, 12, 10, 35, 0, TimeSpan.Zero),
            null,
            0,
            null);

    private static Share CreateFileShare(string code, ShareStatus status, int downloadCount, int? maxDownloadCount) =>
        new(
            Guid.NewGuid(),
            code,
            ShareMode.StoredShare,
            ShareKind.File,
            status,
            null,
            null,
            null,
            "report.txt",
            "text/plain",
            12,
            "shares/FILE0001/report.txt",
            new DateTimeOffset(2026, 4, 12, 10, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2026, 4, 12, 10, 35, 0, TimeSpan.Zero),
            null,
            downloadCount,
            maxDownloadCount);

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

    private sealed class FakeShareUnlockProofService : IShareUnlockProofService
    {
        public string CreateProof(Guid shareId, string code, DateTimeOffset unlockedUntilUtc) => "proof";

        public bool TryValidate(string proof, Guid expectedShareId, string expectedCode, DateTimeOffset now, out DateTimeOffset unlockedUntilUtc)
        {
            unlockedUntilUtc = now.AddMinutes(5);
            return true;
        }
    }
}
