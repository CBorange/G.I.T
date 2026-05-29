using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GIT_Backend.Migrations
{
    /// <inheritdoc />
    public partial class ReMatchingAnalyzerProviderRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "interval_min",
                table: "source_provider");

            migrationBuilder.DropColumn(
                name: "provider_type",
                table: "analyzer_provider");

            migrationBuilder.AddColumn<bool>(
                name: "is_active",
                table: "source_category",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "last_running_at",
                table: "analyzer_provider",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "analyze_job_id",
                table: "analyzed_contents",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<decimal>(
                name: "confidence",
                table: "analyzed_contents",
                type: "numeric(5,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "confidence_reason",
                table: "analyzed_contents",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "updated_at",
                table: "analyzed_contents",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AlterColumn<short>(
                name: "source_provider_id",
                table: "analysis_route",
                type: "smallint",
                nullable: true,
                oldClrType: typeof(short),
                oldType: "smallint");

            migrationBuilder.AddColumn<string>(
                name: "prompt_policy_code",
                table: "analysis_route",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<short>(
                name: "source_category_id",
                table: "analysis_route",
                type: "smallint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: " analyze_jobs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    raw_contents_id = table.Column<Guid>(type: "uuid", nullable: false),
                    analyzer_provider_id = table.Column<short>(type: "smallint", nullable: false),
                    prompt_policy_code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    attempt_count = table.Column<short>(type: "smallint", nullable: false),
                    max_atempt_count = table.Column<short>(type: "smallint", nullable: true),
                    last_error = table.Column<string>(type: "text", nullable: true),
                    last_running_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ended_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_analyze_jobs", x => x.id);
                    table.ForeignKey(
                        name: "FK_analyzer_provider_TO_ analyze_jobs",
                        column: x => x.analyzer_provider_id,
                        principalTable: "analyzer_provider",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_raw_contents_TO_ analyze_jobs",
                        column: x => x.raw_contents_id,
                        principalTable: "raw_contents",
                        principalColumn: "id");
                });

            migrationBuilder.UpdateData(
                table: "source_category",
                keyColumn: "id",
                keyValue: (short)1,
                column: "is_active",
                value: true);

            migrationBuilder.UpdateData(
                table: "source_category",
                keyColumn: "id",
                keyValue: (short)2,
                column: "is_active",
                value: true);

            migrationBuilder.UpdateData(
                table: "source_category",
                keyColumn: "id",
                keyValue: (short)3,
                column: "is_active",
                value: true);

            migrationBuilder.UpdateData(
                table: "source_category",
                keyColumn: "id",
                keyValue: (short)4,
                column: "is_active",
                value: true);

            migrationBuilder.UpdateData(
                table: "source_category",
                keyColumn: "id",
                keyValue: (short)5,
                column: "is_active",
                value: true);

            migrationBuilder.UpdateData(
                table: "source_category",
                keyColumn: "id",
                keyValue: (short)6,
                column: "is_active",
                value: true);

            migrationBuilder.UpdateData(
                table: "source_category",
                keyColumn: "id",
                keyValue: (short)7,
                column: "is_active",
                value: true);

            migrationBuilder.UpdateData(
                table: "source_category",
                keyColumn: "id",
                keyValue: (short)8,
                column: "is_active",
                value: true);

            migrationBuilder.CreateIndex(
                name: "ix_analyzed_contents_analyze_job_id",
                table: "analyzed_contents",
                column: "analyze_job_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_analysis_route_source_category_id",
                table: "analysis_route",
                column: "source_category_id");

            migrationBuilder.CreateIndex(
                name: "ix_analyze_jobs_analyzer_provider_id",
                table: " analyze_jobs",
                column: "analyzer_provider_id");

            migrationBuilder.CreateIndex(
                name: "ix_analyze_jobs_raw_content_id",
                table: " analyze_jobs",
                column: "raw_contents_id",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_source_category_TO_analysis_route",
                table: "analysis_route",
                column: "source_category_id",
                principalTable: "source_category",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_ analyze_jobs_TO_analyzed_contents",
                table: "analyzed_contents",
                column: "analyze_job_id",
                principalTable: " analyze_jobs",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_source_category_TO_analysis_route",
                table: "analysis_route");

            migrationBuilder.DropForeignKey(
                name: "FK_ analyze_jobs_TO_analyzed_contents",
                table: "analyzed_contents");

            migrationBuilder.DropTable(
                name: " analyze_jobs");

            migrationBuilder.DropIndex(
                name: "ix_analyzed_contents_analyze_job_id",
                table: "analyzed_contents");

            migrationBuilder.DropIndex(
                name: "ix_analysis_route_source_category_id",
                table: "analysis_route");

            migrationBuilder.DropColumn(
                name: "is_active",
                table: "source_category");

            migrationBuilder.DropColumn(
                name: "last_running_at",
                table: "analyzer_provider");

            migrationBuilder.DropColumn(
                name: "analyze_job_id",
                table: "analyzed_contents");

            migrationBuilder.DropColumn(
                name: "confidence",
                table: "analyzed_contents");

            migrationBuilder.DropColumn(
                name: "confidence_reason",
                table: "analyzed_contents");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "analyzed_contents");

            migrationBuilder.DropColumn(
                name: "prompt_policy_code",
                table: "analysis_route");

            migrationBuilder.DropColumn(
                name: "source_category_id",
                table: "analysis_route");

            migrationBuilder.AddColumn<int>(
                name: "interval_min",
                table: "source_provider",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "provider_type",
                table: "analyzer_provider",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<short>(
                name: "source_provider_id",
                table: "analysis_route",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0,
                oldClrType: typeof(short),
                oldType: "smallint",
                oldNullable: true);
        }
    }
}
