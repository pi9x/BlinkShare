using BlinkShare.Api.Common.Auth;
using BlinkShare.Api.Common.Time;
using BlinkShare.Api.Features.Shares.CreateFileUpload;
using BlinkShare.Api.Infrastructure.ObjectStorage;
using BlinkShare.Api.Infrastructure.Persistence;
using BlinkShare.Api.Infrastructure.Security;
using BlinkShare.Api.UnitTests.TestDoubles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace BlinkShare.Api.UnitTests.Features.Shares.CreateFileUpload;

public sealed class HandlerTests
{
    [Fact]
    public async Task Missing_file_name_returns_validation_failure()
    {
        var handler = CreateHandler(CreateDbContextFactory(Guid.NewGuid().ToString("N")));

        var result = await handler.HandleAsync(new Command(AccountTier.Free, null, "text/plain", 12), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("general.validation", result.Error!.Code);
    }

    [Fact]
    public async Task Valid_request_creates_pending_file_share()
    {
        var databaseName = Guid.NewGuid().ToString("N");
        var dbContextFactory = CreateDbContextFactory(databaseName);
        var handler = CreateHandler(dbContextFactory, authenticated: true);

        var result = await handler.HandleAsync(
            new Command(AccountTier.Free, "report.txt", "text/plain", 12),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("FILE1234", result.Value!.Code);
        Assert.Contains("/upload/", result.Value.UploadUrl, StringComparison.Ordinal);

        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        var share = await dbContext.Shares.AsNoTracking().SingleAsync();

        Assert.Equal(ShareKind.File, share.Kind);
        Assert.Equal(ShareStatus.Pending, share.Status);
        Assert.Equal("report.txt", share.FileName);
        Assert.Equal(12, share.SizeBytes);
        Assert.NotNull(share.StorageKey);
    }

    [Fact]
    public async Task Anonymous_request_larger_than_limit_returns_validation_failure()
    {
        var handler = CreateHandler(CreateDbContextFactory(Guid.NewGuid().ToString("N")));

        var result = await handler.HandleAsync(
            new Command(AccountTier.Free, "report.txt", "text/plain", (512 * 1024) + 1),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("share.file_too_large", result.Error!.Code);
    }

    [Fact]
    public async Task Authenticated_request_allows_free_limit()
    {
        var databaseName = Guid.NewGuid().ToString("N");
        var dbContextFactory = CreateDbContextFactory(databaseName);
        var handler = CreateHandler(dbContextFactory, authenticated: true);

        var result = await handler.HandleAsync(
            new Command(AccountTier.Anonymous, "report.txt", "text/plain", 1024 * 1024),
            CancellationToken.None);

        Assert.True(result.IsSuccess);

        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        var share = await dbContext.Shares.AsNoTracking().SingleAsync();
        Assert.Equal(1024 * 1024, share.SizeBytes);
    }

    private static Handler CreateHandler(
        IDbContextFactory<BlinkShareDbContext> dbContextFactory,
        bool authenticated = false) =>
        new(
            dbContextFactory,
            new Validator(Options.Create(new CreateFileUploadOptions
            {
                AnonymousMaxFileSizeBytes = 512 * 1024,
                FreeMaxFileSizeBytes = 1024 * 1024,
                FreeTierTtlMinutes = 5
            })),
            new FixedCodeGenerator("FILE1234"),
            new FakeObjectStorage(),
            new FakeCurrentAccountAccessor(authenticated),
            new FakeClock(new DateTimeOffset(2026, 4, 12, 10, 30, 0, TimeSpan.Zero)),
            Options.Create(new CreateFileUploadOptions
            {
                AnonymousMaxFileSizeBytes = 512 * 1024,
                FreeMaxFileSizeBytes = 1024 * 1024,
                FreeTierTtlMinutes = 5
            }));

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

    private sealed class FixedCodeGenerator(string code) : ICodeGenerator
    {
        public string GenerateShareCode() => code;
    }

    private sealed class FakeClock(DateTimeOffset utcNow) : IClock
    {
        public DateTimeOffset UtcNow { get; } = utcNow;
    }

    private sealed class FakeCurrentAccountAccessor(bool authenticated) : ICurrentAccountAccessor
    {
        public CurrentAccount? GetCurrentAccount() =>
            authenticated ? new CurrentAccount(Guid.NewGuid(), "tester@example.com", AccountTier.Free) : null;
    }
}
