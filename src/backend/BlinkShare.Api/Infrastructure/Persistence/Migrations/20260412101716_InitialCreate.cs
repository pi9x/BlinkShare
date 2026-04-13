using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BlinkShare.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "shares",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    tier = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    mode = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    kind = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    status = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    owner_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    passcode_hash = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    text_inline = table.Column<string>(type: "text", nullable: true),
                    file_name = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    content_type = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    size_bytes = table.Column<long>(type: "bigint", nullable: false),
                    storage_key = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    expires_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_accessed_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    download_count = table.Column<int>(type: "integer", nullable: false),
                    max_download_count = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_shares", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_shares_code",
                table: "shares",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_shares_expires_at_utc",
                table: "shares",
                column: "expires_at_utc");

            migrationBuilder.CreateIndex(
                name: "ix_shares_owner_user_id",
                table: "shares",
                column: "owner_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_shares_status_expires_at_utc",
                table: "shares",
                columns: new[] { "status", "expires_at_utc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "shares");
        }
    }
}
