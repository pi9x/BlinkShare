using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlinkShare.Api.Infrastructure.Persistence.Configurations;

public sealed class ShareConfiguration : IEntityTypeConfiguration<Share>
{
    public void Configure(EntityTypeBuilder<Share> builder)
    {
        builder.ToTable("shares");

        builder.HasKey(share => share.Id);

        builder.Property(share => share.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(share => share.Code)
            .HasColumnName("code")
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(share => share.Tier)
            .HasColumnName("tier")
            .HasConversion<string>()
            .HasMaxLength(16)
            .IsRequired();

        builder.Property(share => share.Mode)
            .HasColumnName("mode")
            .HasConversion<string>()
            .HasMaxLength(24)
            .IsRequired();

        builder.Property(share => share.Kind)
            .HasColumnName("kind")
            .HasConversion<string>()
            .HasMaxLength(16)
            .IsRequired();

        builder.Property(share => share.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(16)
            .IsRequired();

        builder.Property(share => share.OwnerUserId)
            .HasColumnName("owner_user_id");

        builder.Property(share => share.PasscodeHash)
            .HasColumnName("passcode_hash")
            .HasMaxLength(256);

        builder.Property(share => share.TextInline)
            .HasColumnName("text_inline");

        builder.Property(share => share.FileName)
            .HasColumnName("file_name")
            .HasMaxLength(512);

        builder.Property(share => share.ContentType)
            .HasColumnName("content_type")
            .HasMaxLength(255);

        builder.Property(share => share.SizeBytes)
            .HasColumnName("size_bytes")
            .IsRequired();

        builder.Property(share => share.StorageKey)
            .HasColumnName("storage_key")
            .HasMaxLength(512);

        builder.Property(share => share.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();

        builder.Property(share => share.ExpiresAtUtc)
            .HasColumnName("expires_at_utc");

        builder.Property(share => share.LastAccessedAtUtc)
            .HasColumnName("last_accessed_at_utc");

        builder.Property(share => share.DownloadCount)
            .HasColumnName("download_count")
            .IsRequired();

        builder.Property(share => share.MaxDownloadCount)
            .HasColumnName("max_download_count");

        builder.Property(share => share.StorageCleanupCompletedAtUtc)
            .HasColumnName("storage_cleanup_completed_at_utc");

        builder.HasIndex(share => share.Code)
            .HasDatabaseName("ix_shares_code")
            .IsUnique();

        builder.HasIndex(share => share.ExpiresAtUtc)
            .HasDatabaseName("ix_shares_expires_at_utc");

        builder.HasIndex(share => new { share.Status, share.ExpiresAtUtc })
            .HasDatabaseName("ix_shares_status_expires_at_utc");

        builder.HasIndex(share => share.OwnerUserId)
            .HasDatabaseName("ix_shares_owner_user_id");

        builder.HasIndex(share => share.StorageCleanupCompletedAtUtc)
            .HasDatabaseName("ix_shares_storage_cleanup_completed_at_utc");
    }
}
