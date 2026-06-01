using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GIT_Backend.Migrations
{
    /// <inheritdoc />
    public partial class AnalysisRouteAddIsDefault : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_default",
                table: "analysis_route",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_default",
                table: "analysis_route");
        }
    }
}
