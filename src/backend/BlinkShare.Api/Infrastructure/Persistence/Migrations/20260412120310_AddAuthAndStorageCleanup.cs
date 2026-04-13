using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BlinkShare.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAuthAndStorageCleanup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "storage_cleanup_completed_at_utc",
                table: "shares",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "accounts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    normalized_email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    password_hash = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    last_login_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_accounts", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "auth_sessions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    account_id = table.Column<Guid>(type: "uuid", nullable: false),
                    token_hash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    expires_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    last_seen_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    revoked_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_auth_sessions", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_shares_storage_cleanup_completed_at_utc",
                table: "shares",
                column: "storage_cleanup_completed_at_utc");

            migrationBuilder.CreateIndex(
                name: "ix_accounts_normalized_email",
                table: "accounts",
                column: "normalized_email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_auth_sessions_account_id_expires_at_utc",
                table: "auth_sessions",
                columns: new[] { "account_id", "expires_at_utc" });

            migrationBuilder.CreateIndex(
                name: "ix_auth_sessions_revoked_at_utc",
                table: "auth_sessions",
                column: "revoked_at_utc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "accounts");

            migrationBuilder.DropTable(
                name: "auth_sessions");

            migrationBuilder.DropIndex(
                name: "ix_shares_storage_cleanup_completed_at_utc",
                table: "shares");

            migrationBuilder.DropColumn(
                name: "storage_cleanup_completed_at_utc",
                table: "shares");
        }
    }
}
