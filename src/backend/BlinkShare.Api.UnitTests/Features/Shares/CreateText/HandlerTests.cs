using BlinkShare.Api.Common.Auth;
using BlinkShare.Api.Common.Time;
using BlinkShare.Api.Features.Shares.CreateText;
using BlinkShare.Api.Infrastructure.Persistence;
using BlinkShare.Api.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace BlinkShare.Api.UnitTests.Features.Shares.CreateText;

public sealed class HandlerTests
{
    [Fact]
    public async Task Valid_request_succeeds()
    {
        var databaseName = Guid.NewGuid().ToString("N");
        var dbContextFactory = CreateDbContextFactory(databaseName);
        var clock = new FakeClock(new DateTimeOffset(2026, 4, 12, 10, 30, 0, TimeSpan.Zero));
        var handler = CreateHandler(dbContextFactory, clock, new FixedCodeGenerator("TEXT1234"), maxTextLength: 1000);
        var command = new Command(AccountTier.Free, "hello");

        var result = await handler.HandleAsync(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("TEXT1234", result.Value!.Code);
        Assert.Equal(clock.UtcNow.AddMinutes(5), result.Value.ExpiresAtUtc);

        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        var share = await dbContext.Shares.AsNoTracking().SingleAsync();

        Assert.Equal(result.Value.ShareId, share.Id);
        Assert.Equal(ShareMode.StoredShare, share.Mode);
        Assert.Equal(ShareKind.Text, share.Kind);
        Assert.Equal(ShareStatus.Ready, share.Status);
        Assert.Equal("hello", share.TextInline);
        Assert.Equal(5, share.SizeBytes);
    }

    [Fact]
    public async Task Blank_text_fails()
    {
        var handler = CreateHandler(CreateDbContextFactory(Guid.NewGuid().ToString("N")));

        var result = await handler.HandleAsync(new Command(AccountTier.Free, "   "), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("share.invalid_text", result.Error!.Code);
    }

    [Fact]
    public async Task Missing_text_fails()
    {
        var handler = CreateHandler(CreateDbContextFactory(Guid.NewGuid().ToString("N")));

        var result = await handler.HandleAsync(new Command(AccountTier.Free, null), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("general.validation", result.Error!.Code);
    }

    [Fact]
    public async Task Too_large_text_fails()
    {
        var handler = CreateHandler(CreateDbContextFactory(Guid.NewGuid().ToString("N")), maxTextLength: 3);

        var result = await handler.HandleAsync(new Command(AccountTier.Free, "toolong"), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("share.text_too_large", result.Error!.Code);
    }

    [Fact]
    public async Task Anonymous_tier_fails()
    {
        var handler = CreateHandler(CreateDbContextFactory(Guid.NewGuid().ToString("N")), accountTier: null);

        var result = await handler.HandleAsync(new Command(AccountTier.Anonymous, "hello"), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("share.anonymous_relay_not_supported", result.Error!.Code);
    }

    [Fact]
    public async Task Existing_code_is_retried_until_unique_code_is_found()
    {
        var databaseName = Guid.NewGuid().ToString("N");
        var dbContextFactory = CreateDbContextFactory(databaseName);

        await using (var seedContext = await dbContextFactory.CreateDbContextAsync())
        {
            await seedContext.Shares.AddAsync(new Share(
                id: Guid.NewGuid(),
                code: "DUPLICATE",
                mode: ShareMode.StoredShare,
                kind: ShareKind.Text,
                status: ShareStatus.Ready,
                ownerUserId: null,
                passcodeHash: null,
                textInline: "seed",
                fileName: null,
                contentType: "text/plain; charset=utf-8",
                sizeBytes: 4,
                storageKey: null,
                createdAtUtc: new DateTimeOffset(2026, 4, 12, 10, 0, 0, TimeSpan.Zero),
                expiresAtUtc: new DateTimeOffset(2026, 4, 12, 10, 5, 0, TimeSpan.Zero),
                lastAccessedAtUtc: null,
                downloadCount: 0,
                maxDownloadCount: null));
            await seedContext.SaveChangesAsync();
        }

        var handler = CreateHandler(
            dbContextFactory,
            codeGenerator: new SequenceCodeGenerator("DUPLICATE", "UNIQUE123"));

        var result = await handler.HandleAsync(new Command(AccountTier.Free, "hello"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("UNIQUE123", result.Value!.Code);
    }

    private static Handler CreateHandler(
        IDbContextFactory<BlinkShareDbContext> dbContextFactory,
        IClock? clock = null,
        ICodeGenerator? codeGenerator = null,
        AccountTier? accountTier = AccountTier.Free,
        int maxTextLength = 10_000)
    {
        var options = Options.Create(new CreateTextOptions
        {
            MaxTextLength = maxTextLength,
            FreeTierTtlMinutes = 5
        });

        return new Handler(
            dbContextFactory,
            new Validator(options),
            codeGenerator ?? new FixedCodeGenerator("TEXT1234"),
            new FakeCurrentAccountAccessor(accountTier),
            clock ?? new FakeClock(new DateTimeOffset(2026, 4, 12, 10, 30, 0, TimeSpan.Zero)),
            options);
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

    private sealed class FakeCurrentAccountAccessor(AccountTier? tier) : ICurrentAccountAccessor
    {
        public CurrentAccount? GetCurrentAccount() =>
            tier is null ? null : new CurrentAccount(Guid.NewGuid(), "tester@example.com", tier.Value);
    }

    private sealed class FixedCodeGenerator(string code) : ICodeGenerator
    {
        public string GenerateShareCode() => code;
    }

    private sealed class SequenceCodeGenerator(params string[] codes) : ICodeGenerator
    {
        private readonly Queue<string> _codes = new(codes);

        public string GenerateShareCode() => _codes.Dequeue();
    }
}
