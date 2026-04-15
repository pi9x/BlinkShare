using BlinkShare.Api.Common.Time;
using BlinkShare.Api.Features.Auth.Login;
using BlinkShare.Api.Infrastructure.Auth;
using BlinkShare.Api.Infrastructure.Persistence;
using BlinkShare.Api.Infrastructure.Security;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace BlinkShare.Api.UnitTests.Features.Auth.Login;

public sealed class HandlerTests
{
    [Fact]
    public async Task Invalid_credentials_return_failure()
    {
        var dbContextFactory = CreateDbContextFactory(Guid.NewGuid().ToString("N"));
        var passwordHasher = new Pbkdf2AccountPasswordHasher();

        await using (var dbContext = await dbContextFactory.CreateDbContextAsync())
        {
            await dbContext.Accounts.AddAsync(new Account(
                Guid.NewGuid(),
                "demo@example.com",
                BlinkShare.Api.Features.Auth.Register.Handler.NormalizeEmail("demo@example.com"),
                passwordHasher.Hash("secret"),
                AccountTier.Free,
                new DateTimeOffset(2026, 4, 12, 10, 0, 0, TimeSpan.Zero),
                null));
            await dbContext.SaveChangesAsync();
        }

        var handler = CreateHandler(dbContextFactory, passwordHasher);

        var result = await handler.HandleAsync(new Request("demo@example.com", "wrong"), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("auth.invalid_credentials", result.Error!.Code);
    }

    [Fact]
    public async Task Empty_password_can_log_in_when_it_matches()
    {
        var dbContextFactory = CreateDbContextFactory(Guid.NewGuid().ToString("N"));
        var passwordHasher = new Pbkdf2AccountPasswordHasher();

        await using (var dbContext = await dbContextFactory.CreateDbContextAsync())
        {
            await dbContext.Accounts.AddAsync(new Account(
                Guid.NewGuid(),
                "demo@example.com",
                BlinkShare.Api.Features.Auth.Register.Handler.NormalizeEmail("demo@example.com"),
                passwordHasher.Hash(string.Empty),
                AccountTier.Free,
                new DateTimeOffset(2026, 4, 12, 10, 0, 0, TimeSpan.Zero),
                null));
            await dbContext.SaveChangesAsync();
        }

        var handler = CreateHandler(dbContextFactory, passwordHasher);

        var result = await handler.HandleAsync(new Request("demo@example.com", string.Empty), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("demo@example.com", result.Value!.Email);
    }

    private static Handler CreateHandler(
        IDbContextFactory<BlinkShareDbContext> dbContextFactory,
        IAccountPasswordHasher passwordHasher) =>
        new(
            dbContextFactory,
            passwordHasher,
            new FixedAuthSessionTokenFactory("token"),
            new FakeClock(new DateTimeOffset(2026, 4, 12, 10, 30, 0, TimeSpan.Zero)),
            Options.Create(new AuthSessionOptions { SessionLifetimeHours = 24 }));

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

    private sealed class FixedAuthSessionTokenFactory(string token) : IAuthSessionTokenFactory
    {
        public string CreateToken() => token;
    }

    private sealed class FakeClock(DateTimeOffset utcNow) : IClock
    {
        public DateTimeOffset UtcNow { get; } = utcNow;
    }
}
