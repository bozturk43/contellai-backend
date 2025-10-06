using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SocialMediaAssistant.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCoinBalanceToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CoinBalance",
                table: "Users",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CoinBalance",
                table: "Users");
        }
    }
}
