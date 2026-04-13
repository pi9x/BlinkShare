using BlinkShare.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BlinkShare.Api.UnitTests.Infrastructure.Persistence;

public sealed class ShareConfigurationTests
{
    [Fact]
    public void Share_model_uses_expected_table_columns_and_indexes()
    {
        var options = new DbContextOptionsBuilder<BlinkShareDbContext>()
            .UseNpgsql("Host=localhost;Port=5432;Database=blinkshare_tests;Username=postgres;Password=postgres")
            .Options;

        using var dbContext = new BlinkShareDbContext(options);
        var entityType = dbContext.Model.FindEntityType(typeof(Share));

        Assert.NotNull(entityType);
        Assert.Equal("shares", entityType!.GetTableName());
        Assert.Equal("code", entityType.FindProperty(nameof(Share.Code))!.GetColumnName());
        Assert.Equal("owner_user_id", entityType.FindProperty(nameof(Share.OwnerUserId))!.GetColumnName());
        Assert.Equal("status", entityType.FindProperty(nameof(Share.Status))!.GetColumnName());
        Assert.Contains(entityType.GetIndexes(), index => index.GetDatabaseName() == "ix_shares_code" && index.IsUnique);
        Assert.Contains(entityType.GetIndexes(), index => index.GetDatabaseName() == "ix_shares_expires_at_utc");
        Assert.Contains(entityType.GetIndexes(), index => index.GetDatabaseName() == "ix_shares_status_expires_at_utc");
        Assert.Contains(entityType.GetIndexes(), index => index.GetDatabaseName() == "ix_shares_owner_user_id");
    }
}
