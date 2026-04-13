using BlinkShare.Api.Common.Time;
using BlinkShare.Api.Features.Shares.GetByCode;
using BlinkShare.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BlinkShare.Api.UnitTests.Features.Shares.GetByCode;

public sealed class HandlerTests
{
    [Fact]
    public async Task Missing_code_returns_not_found()
    {
        var handler = CreateHandler(CreateDbContextFactory(Guid.NewGuid().ToString("N")));

        var result = await handler.HandleAsync("UNKNOWN", CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("share.not_found", result.Error!.Code);
    }

    [Fact]
    public async Task Expired_share_returns_expired()
    {
        var databaseName = Guid.NewGuid().ToString("N");
        var dbContextFactory = CreateDbContextFactory(databaseName);

        await SeedShareAsync(
            dbContextFactory,
            new Share(
                id: Guid.NewGuid(),
                code: "EXPIRED1",
                tier: ShareTier.Free,
                mode: ShareMode.StoredShare,
                kind: ShareKind.Text,
                status: ShareStatus.Ready,
                ownerUserId: null,
                passcodeHash: null,
                textInline: "expired",
                fileName: null,
                contentType: "text/plain; charset=utf-8",
                sizeBytes: 7,
                storageKey: null,
                createdAtUtc: new DateTimeOffset(2026, 4, 12, 9, 0, 0, TimeSpan.Zero),
                expiresAtUtc: new DateTimeOffset(2026, 4, 12, 10, 29, 59, TimeSpan.Zero),
                lastAccessedAtUtc: null,
                downloadCount: 0,
                maxDownloadCount: null));

        var handler = CreateHandler(
            dbContextFactory,
            new FakeClock(new DateTimeOffset(2026, 4, 12, 10, 30, 0, TimeSpan.Zero)));

        var result = await handler.HandleAsync("EXPIRED1", CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("share.expired", result.Error!.Code);
    }

    [Fact]
    public async Task Existing_code_returns_public_metadata()
    {
        var databaseName = Guid.NewGuid().ToString("N");
        var dbContextFactory = CreateDbContextFactory(databaseName);
        var shareId = Guid.NewGuid();

        await SeedShareAsync(
            dbContextFactory,
            new Share(
                id: shareId,
                code: "PUBLIC01",
                tier: ShareTier.Free,
                mode: ShareMode.StoredShare,
                kind: ShareKind.Text,
                status: ShareStatus.Ready,
                ownerUserId: null,
                passcodeHash: "hash",
                textInline: "hello",
                fileName: null,
                contentType: "text/plain; charset=utf-8",
                sizeBytes: 5,
                storageKey: null,
                createdAtUtc: new DateTimeOffset(2026, 4, 12, 9, 0, 0, TimeSpan.Zero),
                expiresAtUtc: new DateTimeOffset(2026, 4, 12, 10, 35, 0, TimeSpan.Zero),
                lastAccessedAtUtc: null,
                downloadCount: 0,
                maxDownloadCount: null));

        var handler = CreateHandler(
            dbContextFactory,
            new FakeClock(new DateTimeOffset(2026, 4, 12, 10, 30, 0, TimeSpan.Zero)));

        var result = await handler.HandleAsync("PUBLIC01", CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(shareId, result.Value!.ShareId);
        Assert.Equal("PUBLIC01", result.Value.Code);
        Assert.Equal(ShareKind.Text, result.Value.Kind);
        Assert.Equal(ShareStatus.Ready, result.Value.Status);
        Assert.Equal(new DateTimeOffset(2026, 4, 12, 10, 35, 0, TimeSpan.Zero), result.Value.ExpiresAtUtc);
        Assert.True(result.Value.HasPasscode);
        Assert.Equal(5, result.Value.SizeBytes);
    }

    private static Handler CreateHandler(
        IDbContextFactory<BlinkShareDbContext> dbContextFactory,
        IClock? clock = null) =>
        new(
            dbContextFactory,
            clock ?? new FakeClock(new DateTimeOffset(2026, 4, 12, 10, 30, 0, TimeSpan.Zero)));

    private static IDbContextFactory<BlinkShareDbContext> CreateDbContextFactory(string databaseName)
    {
        var options = new DbContextOptionsBuilder<BlinkShareDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;

        return new TestDbContextFactory(options);
    }

    private static async Task SeedShareAsync(
        IDbContextFactory<BlinkShareDbContext> dbContextFactory,
        Share share)
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
