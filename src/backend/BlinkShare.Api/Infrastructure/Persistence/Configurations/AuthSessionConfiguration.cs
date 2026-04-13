using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlinkShare.Api.Infrastructure.Persistence.Configurations;

public sealed class AuthSessionConfiguration : IEntityTypeConfiguration<AuthSession>
{
    public void Configure(EntityTypeBuilder<AuthSession> builder)
    {
        builder.ToTable("auth_sessions");

        builder.HasKey(session => session.Id);

        builder.Property(session => session.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(session => session.AccountId)
            .HasColumnName("account_id")
            .IsRequired();

        builder.Property(session => session.TokenHash)
            .HasColumnName("token_hash")
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(session => session.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();

        builder.Property(session => session.ExpiresAtUtc)
            .HasColumnName("expires_at_utc")
            .IsRequired();

        builder.Property(session => session.LastSeenAtUtc)
            .HasColumnName("last_seen_at_utc");

        builder.Property(session => session.RevokedAtUtc)
            .HasColumnName("revoked_at_utc");

        builder.HasIndex(session => new { session.AccountId, session.ExpiresAtUtc })
            .HasDatabaseName("ix_auth_sessions_account_id_expires_at_utc");

        builder.HasIndex(session => session.RevokedAtUtc)
            .HasDatabaseName("ix_auth_sessions_revoked_at_utc");
    }
}
