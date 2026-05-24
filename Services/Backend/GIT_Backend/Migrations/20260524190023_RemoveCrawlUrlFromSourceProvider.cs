using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GIT_Backend.Migrations
{
    /// <inheritdoc />
    public partial class RemoveCrawlUrlFromSourceProvider : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "crawl_url",
                table: "source_provider");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "crawl_url",
                table: "source_provider",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
