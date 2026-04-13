using BlinkShare.Api.Common.Time;
using BlinkShare.Api.Features.Shares.ReadText;
using BlinkShare.Api.Infrastructure.Persistence;
using BlinkShare.Api.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;

namespace BlinkShare.Api.UnitTests.Features.Shares.ReadText;

public sealed class HandlerTests
{
    [Fact]
    public async Task Protected_share_requires_unlock_proof()
    {
        var databaseName = Guid.NewGuid().ToString("N");
        var dbContextFactory = CreateDbContextFactory(databaseName);

        await SeedShareAsync(
            dbContextFactory,
            new Share(
                Guid.NewGuid(),
                "LOCKED01",
                ShareTier.Free,
                ShareMode.StoredShare,
                ShareKind.Text,
                ShareStatus.Ready,
                null,
                "hash",
                "secret",
                null,
                "text/plain; charset=utf-8",
                6,
                null,
                new DateTimeOffset(2026, 4, 12, 10, 0, 0, TimeSpan.Zero),
                new DateTimeOffset(2026, 4, 12, 10, 35, 0, TimeSpan.Zero),
                null,
                0,
                null));

        var handler = new Handler(
            dbContextFactory,
            new FakeClock(new DateTimeOffset(2026, 4, 12, 10, 30, 0, TimeSpan.Zero)),
            new FakeShareUnlockProofService(false));

        var result = await handler.HandleAsync("LOCKED01", null, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("share.passcode_required", result.Error!.Code);
    }

    [Fact]
    public async Task Non_text_share_returns_invalid_kind()
    {
        var databaseName = Guid.NewGuid().ToString("N");
        var dbContextFactory = CreateDbContextFactory(databaseName);

        await SeedShareAsync(
            dbContextFactory,
            new Share(
                Guid.NewGuid(),
                "FILE0001",
                ShareTier.Free,
                ShareMode.StoredShare,
                ShareKind.File,
                ShareStatus.Ready,
                null,
                null,
                null,
                "demo.txt",
                "text/plain",
                4,
                "shares/FILE0001/demo.txt",
                new DateTimeOffset(2026, 4, 12, 10, 0, 0, TimeSpan.Zero),
                new DateTimeOffset(2026, 4, 12, 10, 35, 0, TimeSpan.Zero),
                null,
                0,
                null));

        var handler = new Handler(
            dbContextFactory,
            new FakeClock(new DateTimeOffset(2026, 4, 12, 10, 30, 0, TimeSpan.Zero)),
            new FakeShareUnlockProofService(true));

        var result = await handler.HandleAsync("FILE0001", null, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("share.invalid_kind", result.Error!.Code);
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

    private sealed class FakeShareUnlockProofService(bool isValid) : IShareUnlockProofService
    {
        public string CreateProof(Guid shareId, string code, DateTimeOffset unlockedUntilUtc) => "proof";

        public bool TryValidate(string proof, Guid expectedShareId, string expectedCode, DateTimeOffset now, out DateTimeOffset unlockedUntilUtc)
        {
            unlockedUntilUtc = now.AddMinutes(5);
            return isValid;
        }
    }
}
