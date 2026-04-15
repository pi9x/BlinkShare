using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BlinkShare.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(BlinkShareDbContext))]
    [Migration("20260415062000_MoveTierFromSharesToAccounts")]
    public partial class MoveTierFromSharesToAccounts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "tier",
                table: "accounts",
                type: "character varying(16)",
                maxLength: 16,
                nullable: false,
                defaultValue: "Free");

            migrationBuilder.DropColumn(
                name: "tier",
                table: "shares");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "tier",
                table: "shares",
                type: "character varying(16)",
                maxLength: 16,
                nullable: false,
                defaultValue: "Free");

            migrationBuilder.DropColumn(
                name: "tier",
                table: "accounts");
        }
    }
}
