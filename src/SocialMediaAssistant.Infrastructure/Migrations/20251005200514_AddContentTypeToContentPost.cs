using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SocialMediaAssistant.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddContentTypeToContentPost : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ContentType",
                table: "ContentPosts",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContentType",
                table: "ContentPosts");
        }
    }
}
