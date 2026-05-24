using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

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
                    provider_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    model_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    endpoint_url = table.Column<string>(type: "text", nullable: true),
                    is_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    config_json = table.Column<string>(type: "jsonb", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
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
                    expect_category_id = table.Column<short>(type: "smallint", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    base_url = table.Column<string>(type: "text", nullable: false),
                    crawl_url = table.Column<string>(type: "text", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    interval_min = table.Column<int>(type: "integer", nullable: false),
                    request_delay_ms = table.Column<int>(type: "integer", nullable: false),
                    description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_source_provider", x => x.id);
                    table.ForeignKey(
                        name: "FK_source_category_TO_source_provider",
                        column: x => x.expect_category_id,
                        principalTable: "source_category",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "analysis_route",
                columns: table => new
                {
                    id = table.Column<short>(type: "smallint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    source_provider_id = table.Column<short>(type: "smallint", nullable: false),
                    analyzer_provider_id = table.Column<short>(type: "smallint", nullable: false),
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
                        name: "FK_source_provider_TO_analysis_route",
                        column: x => x.source_provider_id,
                        principalTable: "source_provider",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "raw_contents",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    source_provider_id = table.Column<short>(type: "smallint", nullable: false),
                    expect_category_id = table.Column<short>(type: "smallint", nullable: false),
                    source_url = table.Column<string>(type: "text", nullable: false),
                    content_id = table.Column<string>(type: "text", maxLength: 50, nullable: false),
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
                        name: "FK_source_category_TO_raw_contents",
                        column: x => x.expect_category_id,
                        principalTable: "source_category",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_source_provider_TO_raw_contents",
                        column: x => x.source_provider_id,
                        principalTable: "source_provider",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "analyzed_contents",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    raw_content_id = table.Column<Guid>(type: "uuid", nullable: false),
                    analyzer_provider_id = table.Column<short>(type: "smallint", nullable: false),
                    actual_category_id = table.Column<short>(type: "smallint", nullable: false),
                    title_summary = table.Column<string>(type: "text", nullable: false),
                    body_summary = table.Column<string>(type: "text", nullable: false),
                    keyword_json = table.Column<string>(type: "jsonb", nullable: true),
                    location_json = table.Column<string>(type: "jsonb", nullable: true),
                    model_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    analysis_payload_json = table.Column<string>(type: "jsonb", nullable: true),
                    analyzed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_analyzed_contents", x => x.id);
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

            migrationBuilder.InsertData(
                table: "source_category",
                columns: new[] { "id", "code", "description", "name", "updated_at" },
                values: new object[,]
                {
                    { (short)1, "culture", "문화 관련 이슈", "문화", null },
                    { (short)2, "economy", "경제 관련 이슈", "경제", null },
                    { (short)3, "welfare", "복지 관련 이슈", "복지", null },
                    { (short)4, "transport", "교통 관련 이슈", "교통", null },
                    { (short)5, "environment", "환경 관련 이슈", "환경", null },
                    { (short)6, "housing", "주택 관련 이슈", "주택", null },
                    { (short)7, "safety", "안전 관련 이슈", "안전", null },
                    { (short)8, "administration", "행정 관련 이슈", "행정", null }
                });

            migrationBuilder.CreateIndex(
                name: "ix_analysis_route_analyzer_provider_id",
                table: "analysis_route",
                column: "analyzer_provider_id");

            migrationBuilder.CreateIndex(
                name: "UQ_analysis_route_source_analyzer",
                table: "analysis_route",
                columns: new[] { "source_provider_id", "analyzer_provider_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_analyzed_contents_actual_category_id",
                table: "analyzed_contents",
                column: "actual_category_id");

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
                name: "ix_raw_contents_content_id",
                table: "raw_contents",
                column: "content_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_raw_contents_expect_category_id",
                table: "raw_contents",
                column: "expect_category_id");

            migrationBuilder.CreateIndex(
                name: "ix_raw_contents_source_provider_id",
                table: "raw_contents",
                column: "source_provider_id");

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

            migrationBuilder.CreateIndex(
                name: "ix_source_provider_expect_category_id",
                table: "source_provider",
                column: "expect_category_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "analysis_route");

            migrationBuilder.DropTable(
                name: "analyzed_contents");

            migrationBuilder.DropTable(
                name: "analyzer_provider");

            migrationBuilder.DropTable(
                name: "raw_contents");

            migrationBuilder.DropTable(
                name: "source_provider");

            migrationBuilder.DropTable(
                name: "source_category");
        }
    }
}
