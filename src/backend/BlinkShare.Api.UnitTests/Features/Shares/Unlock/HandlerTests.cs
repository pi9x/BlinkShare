using BlinkShare.Api.Common.Time;
using BlinkShare.Api.Features.Shares.Unlock;
using BlinkShare.Api.Infrastructure.Persistence;
using BlinkShare.Api.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace BlinkShare.Api.UnitTests.Features.Shares.Unlock;

public sealed class HandlerTests
{
    [Fact]
    public async Task Invalid_passcode_returns_failure()
    {
        var hasher = new Pbkdf2PasscodeHasher();
        var databaseName = Guid.NewGuid().ToString("N");
        var dbContextFactory = CreateDbContextFactory(databaseName);

        await SeedShareAsync(
            dbContextFactory,
            new Share(
                Guid.NewGuid(),
                "LOCK1234",
                ShareMode.StoredShare,
                ShareKind.Text,
                ShareStatus.Ready,
                null,
                hasher.Hash("secret"),
                "hidden",
                null,
                "text/plain; charset=utf-8",
                6,
                null,
                new DateTimeOffset(2026, 4, 12, 10, 0, 0, TimeSpan.Zero),
                new DateTimeOffset(2026, 4, 12, 10, 35, 0, TimeSpan.Zero),
                null,
                0,
                null));

        var handler = CreateHandler(dbContextFactory, hasher);

        var result = await handler.HandleAsync("LOCK1234", "wrong", CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("share.invalid_passcode", result.Error!.Code);
    }

    [Fact]
    public async Task Expired_share_returns_failure()
    {
        var hasher = new Pbkdf2PasscodeHasher();
        var databaseName = Guid.NewGuid().ToString("N");
        var dbContextFactory = CreateDbContextFactory(databaseName);

        await SeedShareAsync(
            dbContextFactory,
            new Share(
                Guid.NewGuid(),
                "LOCK1234",
                ShareMode.StoredShare,
                ShareKind.Text,
                ShareStatus.Ready,
                null,
                hasher.Hash("secret"),
                "hidden",
                null,
                "text/plain; charset=utf-8",
                6,
                null,
                new DateTimeOffset(2026, 4, 12, 10, 0, 0, TimeSpan.Zero),
                new DateTimeOffset(2026, 4, 12, 10, 29, 0, TimeSpan.Zero),
                null,
                0,
                null));

        var handler = CreateHandler(dbContextFactory, hasher);

        var result = await handler.HandleAsync("LOCK1234", "secret", CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("share.expired", result.Error!.Code);
    }

    private static Handler CreateHandler(
        IDbContextFactory<BlinkShareDbContext> dbContextFactory,
        IPasscodeHasher passcodeHasher) =>
        new(
            dbContextFactory,
            passcodeHasher,
            new FakeShareUnlockProofService(),
            new FakeClock(new DateTimeOffset(2026, 4, 12, 10, 30, 0, TimeSpan.Zero)),
            Options.Create(new ShareUnlockProofOptions { LifetimeMinutes = 5 }));

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
