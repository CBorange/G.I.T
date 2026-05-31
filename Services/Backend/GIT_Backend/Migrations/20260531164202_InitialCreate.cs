using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GIT_Backend.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "analyzer_provider",
                columns: table => new
                {
                    id = table.Column<short>(type: "smallint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    model_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    endpoint_url = table.Column<string>(type: "text", nullable: true),
                    is_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    config_json = table.Column<string>(type: "jsonb", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_running_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_analyzer_provider", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "source_category",
                columns: table => new
                {
                    id = table.Column<short>(type: "smallint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_source_category", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "source_provider",
                columns: table => new
                {
                    id = table.Column<short>(type: "smallint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    base_url = table.Column<string>(type: "text", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    request_delay_ms = table.Column<int>(type: "integer", nullable: false),
                    description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_running_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_source_provider", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "analysis_route",
                columns: table => new
                {
                    id = table.Column<short>(type: "smallint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    source_provider_id = table.Column<short>(type: "smallint", nullable: true),
                    source_category_id = table.Column<short>(type: "smallint", nullable: true),
                    analyzer_provider_id = table.Column<short>(type: "smallint", nullable: false),
                    prompt_policy_code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    is_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_analysis_route", x => x.id);
                    table.ForeignKey(
                        name: "FK_analyzer_provider_TO_analysis_route",
                        column: x => x.analyzer_provider_id,
                        principalTable: "analyzer_provider",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_source_category_TO_analysis_route",
                        column: x => x.source_category_id,
                        principalTable: "source_category",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_source_provider_TO_analysis_route",
                        column: x => x.source_provider_id,
                        principalTable: "source_provider",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "crawl_target",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    source_provider_id = table.Column<short>(type: "smallint", nullable: false),
                    source_category_id = table.Column<short>(type: "smallint", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    entry_url = table.Column<string>(type: "text", nullable: false),
                    request_delay_ms = table.Column<int>(type: "integer", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_running_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_crawl_target", x => x.id);
                    table.ForeignKey(
                        name: "FK_source_category_TO_crawl_target",
                        column: x => x.source_category_id,
                        principalTable: "source_category",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_source_provider_TO_crawl_target",
                        column: x => x.source_provider_id,
                        principalTable: "source_provider",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "raw_contents",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    crawl_target_id = table.Column<int>(type: "integer", nullable: false),
                    source_url = table.Column<string>(type: "text", nullable: false),
                    content_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    author = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    published_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    title = table.Column<string>(type: "text", nullable: false),
                    body = table.Column<string>(type: "text", nullable: true),
                    raw_payload_json = table.Column<string>(type: "jsonb", nullable: true),
                    crawled_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_raw_contents", x => x.id);
                    table.ForeignKey(
                        name: "FK_crawl_target_TO_raw_contents",
                        column: x => x.crawl_target_id,
                        principalTable: "crawl_target",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "analyze_job",
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
                    table.PrimaryKey("pk_analyze_job", x => x.id);
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

            migrationBuilder.CreateTable(
                name: "analyzed_contents",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    raw_content_id = table.Column<Guid>(type: "uuid", nullable: false),
                    analyzer_provider_id = table.Column<short>(type: "smallint", nullable: false),
                    analyze_job_id = table.Column<Guid>(type: "uuid", nullable: false),
                    actual_category_id = table.Column<short>(type: "smallint", nullable: false),
                    title_summary = table.Column<string>(type: "text", nullable: false),
                    body_summary = table.Column<string>(type: "text", nullable: false),
                    keyword_json = table.Column<string>(type: "jsonb", nullable: true),
                    location_json = table.Column<string>(type: "jsonb", nullable: true),
                    model_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    analysis_payload_json = table.Column<string>(type: "jsonb", nullable: true),
                    analyzed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    confidence = table.Column<decimal>(type: "numeric(5,4)", nullable: false),
                    confidence_reason = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_analyzed_contents", x => x.id);
                    table.ForeignKey(
                        name: "FK_ analyze_jobs_TO_analyzed_contents",
                        column: x => x.analyze_job_id,
                        principalTable: "analyze_job",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_analyzer_provider_TO_analyzed_contents",
                        column: x => x.analyzer_provider_id,
                        principalTable: "analyzer_provider",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_raw_contents_TO_analyzed_contents",
                        column: x => x.raw_content_id,
                        principalTable: "raw_contents",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_source_category_TO_analyzed_contents",
                        column: x => x.actual_category_id,
                        principalTable: "source_category",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "ix_analysis_route_analyzer_provider_id",
                table: "analysis_route",
                column: "analyzer_provider_id");

            migrationBuilder.CreateIndex(
                name: "ix_analysis_route_source_category_id",
                table: "analysis_route",
                column: "source_category_id");

            migrationBuilder.CreateIndex(
                name: "UQ_analysis_route_source_analyzer",
                table: "analysis_route",
                columns: new[] { "source_provider_id", "analyzer_provider_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_analyze_job_analyzer_provider_id",
                table: "analyze_job",
                column: "analyzer_provider_id");

            migrationBuilder.CreateIndex(
                name: "ix_analyze_job_raw_content_id",
                table: "analyze_job",
                column: "raw_contents_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_analyzed_contents_actual_category_id",
                table: "analyzed_contents",
                column: "actual_category_id");

            migrationBuilder.CreateIndex(
                name: "ix_analyzed_contents_analyze_job_id",
                table: "analyzed_contents",
                column: "analyze_job_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_analyzed_contents_analyzer_provider_id",
                table: "analyzed_contents",
                column: "analyzer_provider_id");

            migrationBuilder.CreateIndex(
                name: "ix_analyzed_contents_raw_content_id",
                table: "analyzed_contents",
                column: "raw_content_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_analyzer_provider_code",
                table: "analyzer_provider",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_crawl_target_code",
                table: "crawl_target",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_crawl_target_source_category_id",
                table: "crawl_target",
                column: "source_category_id");

            migrationBuilder.CreateIndex(
                name: "ix_crawl_target_source_provider_id",
                table: "crawl_target",
                column: "source_provider_id");

            migrationBuilder.CreateIndex(
                name: "ix_raw_contents_content_id",
                table: "raw_contents",
                column: "content_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_raw_contents_crawl_target_id",
                table: "raw_contents",
                column: "crawl_target_id");

            migrationBuilder.CreateIndex(
                name: "ix_raw_contents_source_url",
                table: "raw_contents",
                column: "source_url",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_source_category_code",
                table: "source_category",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_source_provider_code",
                table: "source_provider",
                column: "code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "analysis_route");

            migrationBuilder.DropTable(
                name: "analyzed_contents");

            migrationBuilder.DropTable(
                name: "analyze_job");

            migrationBuilder.DropTable(
                name: "analyzer_provider");

            migrationBuilder.DropTable(
                name: "raw_contents");

            migrationBuilder.DropTable(
                name: "crawl_target");

            migrationBuilder.DropTable(
                name: "source_category");

            migrationBuilder.DropTable(
                name: "source_provider");
        }
    }
}
