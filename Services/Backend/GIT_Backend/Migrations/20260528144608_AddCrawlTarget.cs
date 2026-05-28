using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GIT_Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddCrawlTarget : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_source_category_TO_raw_contents",
                table: "raw_contents");

            migrationBuilder.DropForeignKey(
                name: "FK_source_provider_TO_raw_contents",
                table: "raw_contents");

            migrationBuilder.DropForeignKey(
                name: "FK_source_category_TO_source_provider",
                table: "source_provider");

            migrationBuilder.DropIndex(
                name: "ix_source_provider_expect_category_id",
                table: "source_provider");

            migrationBuilder.DropIndex(
                name: "ix_raw_contents_expect_category_id",
                table: "raw_contents");

            migrationBuilder.DropIndex(
                name: "ix_raw_contents_source_provider_id",
                table: "raw_contents");

            migrationBuilder.DropColumn(
                name: "expect_category_id",
                table: "source_provider");

            migrationBuilder.DropColumn(
                name: "expect_category_id",
                table: "raw_contents");

            migrationBuilder.DropColumn(
                name: "source_provider_id",
                table: "raw_contents");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "last_running_at",
                table: "source_provider",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "content_id",
                table: "raw_contents",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<int>(
                name: "crawl_target_id",
                table: "raw_contents",
                type: "integer",
                nullable: false,
                defaultValue: 0);

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

            migrationBuilder.CreateIndex(
                name: "ix_raw_contents_crawl_target_id",
                table: "raw_contents",
                column: "crawl_target_id");

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

            migrationBuilder.AddForeignKey(
                name: "FK_crawl_target_TO_raw_contents",
                table: "raw_contents",
                column: "crawl_target_id",
                principalTable: "crawl_target",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_crawl_target_TO_raw_contents",
                table: "raw_contents");

            migrationBuilder.DropTable(
                name: "crawl_target");

            migrationBuilder.DropIndex(
                name: "ix_raw_contents_crawl_target_id",
                table: "raw_contents");

            migrationBuilder.DropColumn(
                name: "last_running_at",
                table: "source_provider");

            migrationBuilder.DropColumn(
                name: "crawl_target_id",
                table: "raw_contents");

            migrationBuilder.AddColumn<short>(
                name: "expect_category_id",
                table: "source_provider",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AlterColumn<string>(
                name: "content_id",
                table: "raw_contents",
                type: "text",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AddColumn<short>(
                name: "expect_category_id",
                table: "raw_contents",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "source_provider_id",
                table: "raw_contents",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.CreateIndex(
                name: "ix_source_provider_expect_category_id",
                table: "source_provider",
                column: "expect_category_id");

            migrationBuilder.CreateIndex(
                name: "ix_raw_contents_expect_category_id",
                table: "raw_contents",
                column: "expect_category_id");

            migrationBuilder.CreateIndex(
                name: "ix_raw_contents_source_provider_id",
                table: "raw_contents",
                column: "source_provider_id");

            migrationBuilder.AddForeignKey(
                name: "FK_source_category_TO_raw_contents",
                table: "raw_contents",
                column: "expect_category_id",
                principalTable: "source_category",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_source_provider_TO_raw_contents",
                table: "raw_contents",
                column: "source_provider_id",
                principalTable: "source_provider",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_source_category_TO_source_provider",
                table: "source_provider",
                column: "expect_category_id",
                principalTable: "source_category",
                principalColumn: "id");
        }
    }
}
