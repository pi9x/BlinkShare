using Microsoft.EntityFrameworkCore;

namespace BlinkShare.Api.Infrastructure.Persistence;

public sealed class BlinkShareDbContext(DbContextOptions<BlinkShareDbContext> options) : DbContext(options)
{
    public DbSet<Account> Accounts => Set<Account>();

    public DbSet<AuthSession> AuthSessions => Set<AuthSession>();

    public DbSet<Share> Shares => Set<Share>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BlinkShareDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
