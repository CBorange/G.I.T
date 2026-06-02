using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GIT_Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddRouteAndConfidenceConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "thumbnail_url",
                table: "raw_contents",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddCheckConstraint(
                name: "CK_analyzed_contents_confidence_range",
                table: "analyzed_contents",
                sql: "confidence >= 0 AND confidence <= 1");

            migrationBuilder.CreateIndex(
                name: "UQ_analysis_route_default_route",
                table: "analysis_route",
                column: "is_default",
                unique: true,
                filter: "is_default = true");

            migrationBuilder.AddCheckConstraint(
                name: "CK_analysis_route_default_route_requires_null_match",
                table: "analysis_route",
                sql: "is_default = false OR (source_provider_id IS NULL AND source_category_id IS NULL)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_analyzed_contents_confidence_range",
                table: "analyzed_contents");

            migrationBuilder.DropIndex(
                name: "UQ_analysis_route_default_route",
                table: "analysis_route");

            migrationBuilder.DropCheckConstraint(
                name: "CK_analysis_route_default_route_requires_null_match",
                table: "analysis_route");

            migrationBuilder.DropColumn(
                name: "thumbnail_url",
                table: "raw_contents");
        }
    }
}
