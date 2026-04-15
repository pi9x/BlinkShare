using BlinkShare.Api.Common.Time;
using BlinkShare.Api.Features.Auth.Register;
using BlinkShare.Api.Infrastructure.Auth;
using BlinkShare.Api.Infrastructure.Persistence;
using BlinkShare.Api.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace BlinkShare.Api.UnitTests.Features.Auth.Register;

public sealed class HandlerTests
{
    [Fact]
    public async Task Duplicate_email_returns_failure()
    {
        var dbContextFactory = CreateDbContextFactory(Guid.NewGuid().ToString("N"));

        await using (var dbContext = await dbContextFactory.CreateDbContextAsync())
        {
            await dbContext.Accounts.AddAsync(new Account(
                Guid.NewGuid(),
                "demo@example.com",
                BlinkShare.Api.Features.Auth.Register.Handler.NormalizeEmail("demo@example.com"),
                new Pbkdf2AccountPasswordHasher().Hash(""),
                AccountTier.Free,
                new DateTimeOffset(2026, 4, 12, 10, 0, 0, TimeSpan.Zero),
                null));
            await dbContext.SaveChangesAsync();
        }

        var handler = CreateHandler(dbContextFactory);

        var result = await handler.HandleAsync(new Command("demo@example.com", string.Empty), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("auth.duplicate_email", result.Error!.Code);
    }

    [Fact]
    public async Task Empty_password_is_accepted()
    {
        var dbContextFactory = CreateDbContextFactory(Guid.NewGuid().ToString("N"));
        var handler = CreateHandler(dbContextFactory);

        var result = await handler.HandleAsync(new Command("demo@example.com", string.Empty), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("demo@example.com", result.Value!.Email);
    }

    private static Handler CreateHandler(IDbContextFactory<BlinkShareDbContext> dbContextFactory) =>
        new(
            dbContextFactory,
            new Validator(),
            new Pbkdf2AccountPasswordHasher(),
            new FixedAuthSessionTokenFactory("token"),
            new FakeClock(new DateTimeOffset(2026, 4, 12, 10, 30, 0, TimeSpan.Zero)),
            Options.Create(new AuthSessionOptions { SessionLifetimeHours = 24 }));

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

    private sealed class FixedAuthSessionTokenFactory(string token) : IAuthSessionTokenFactory
    {
        public string CreateToken() => token;
    }

    private sealed class FakeClock(DateTimeOffset utcNow) : IClock
    {
        public DateTimeOffset UtcNow { get; } = utcNow;
    }
}
