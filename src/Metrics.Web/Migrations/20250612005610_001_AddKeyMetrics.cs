using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Metrics.Web.Migrations
{
    /// <inheritdoc />
    public partial class _001_AddKeyMetrics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_key_kpi_submission_items_departments_department_id",
                schema: "metrics",
                table: "key_kpi_submission_items");

            migrationBuilder.DropForeignKey(
                name: "fk_key_kpi_submission_items_key_kpis_key_kpi_metrics_id",
                schema: "metrics",
                table: "key_kpi_submission_items");

            migrationBuilder.DropForeignKey(
                name: "fk_key_kpi_submissions_departments_department_id",
                schema: "metrics",
                table: "key_kpi_submissions");

            migrationBuilder.DropForeignKey(
                name: "fk_key_kpi_submissions_key_kpis_key_kpi_id",
                schema: "metrics",
                table: "key_kpi_submissions");

            migrationBuilder.DropTable(
                name: "department_key_kpis",
                schema: "metrics");

            migrationBuilder.DropTable(
                name: "key_kpis",
                schema: "metrics");

            migrationBuilder.DropIndex(
                name: "ix_key_kpi_submissions_score_submission_period_id_application_",
                schema: "metrics",
                table: "key_kpi_submissions");

            migrationBuilder.DropIndex(
                name: "ix_key_kpi_submission_items_department_id",
                schema: "metrics",
                table: "key_kpi_submission_items");

            migrationBuilder.DropIndex(
                name: "ix_key_kpi_submission_items_key_kpi_submission_id_key_kpi_metr",
                schema: "metrics",
                table: "key_kpi_submission_items");

            migrationBuilder.DropColumn(
                name: "department_id",
                schema: "metrics",
                table: "key_kpi_submission_items");

            migrationBuilder.DropSequence(
                name: "department_key_kpis_id_seq",
                schema: "metrics");

            migrationBuilder.DropSequence(
                name: "key_kpis_id_seq",
                schema: "metrics");

            migrationBuilder.RenameColumn(
                name: "key_kpi_id",
                schema: "metrics",
                table: "key_kpi_submissions",
                newName: "key_metric_id");

            migrationBuilder.RenameIndex(
                name: "ix_key_kpi_submissions_key_kpi_id",
                schema: "metrics",
                table: "key_kpi_submissions",
                newName: "ix_key_kpi_submissions_key_metric_id");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:citext", ",,");

            migrationBuilder.CreateSequence(
                name: "department_key_metrics_id_seq",
                schema: "metrics",
                incrementBy: 10);

            migrationBuilder.CreateSequence(
                name: "key_metrics_id_seq",
                schema: "metrics",
                incrementBy: 10);

            migrationBuilder.AlterColumn<string>(
                name: "title_name",
                schema: "metrics",
                table: "user_titles",
                type: "citext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar (200)");

            migrationBuilder.AlterColumn<long>(
                name: "department_id",
                schema: "metrics",
                table: "key_kpi_submissions",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AddColumn<long>(
                name: "department_key_metric_id",
                schema: "metrics",
                table: "key_kpi_submissions",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "key_metric_id",
                schema: "metrics",
                table: "key_kpi_submission_items",
                type: "bigint",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "department_name",
                schema: "metrics",
                table: "departments",
                type: "citext",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(200)",
                oldMaxLength: 200);

            migrationBuilder.CreateTable(
                name: "key_metrics",
                schema: "metrics",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    metric_code = table.Column<Guid>(type: "uuid", nullable: false),
                    metric_title = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_key_metrics", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "department_key_metrics",
                schema: "metrics",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    department_key_metric_code = table.Column<Guid>(type: "uuid", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    kpi_submission_period_id = table.Column<long>(type: "bigint", nullable: false),
                    department_id = table.Column<long>(type: "bigint", nullable: false),
                    key_metric_id = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_department_key_metrics", x => x.id);
                    table.ForeignKey(
                        name: "fk_department_key_metrics_departments_department_id",
                        column: x => x.department_id,
                        principalSchema: "metrics",
                        principalTable: "departments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_department_key_metrics_key_metrics_key_metric_id",
                        column: x => x.key_metric_id,
                        principalSchema: "metrics",
                        principalTable: "key_metrics",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_department_key_metrics_kpi_submission_periods_kpi_submissio",
                        column: x => x.kpi_submission_period_id,
                        principalSchema: "metrics",
                        principalTable: "kpi_submission_periods",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_key_kpi_submissions_department_key_metric_id",
                schema: "metrics",
                table: "key_kpi_submissions",
                column: "department_key_metric_id");

            migrationBuilder.CreateIndex(
                name: "ix_key_kpi_submissions_score_submission_period_id_department_i",
                schema: "metrics",
                table: "key_kpi_submissions",
                columns: new[] { "score_submission_period_id", "department_id", "application_user_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_key_kpi_submission_items_key_kpi_submission_id_key_kpi_metr",
                schema: "metrics",
                table: "key_kpi_submission_items",
                columns: new[] { "key_kpi_submission_id", "key_kpi_metrics_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_key_kpi_submission_items_key_metric_id",
                schema: "metrics",
                table: "key_kpi_submission_items",
                column: "key_metric_id");

            migrationBuilder.CreateIndex(
                name: "ix_department_key_metrics_department_id",
                schema: "metrics",
                table: "department_key_metrics",
                column: "department_id");

            migrationBuilder.CreateIndex(
                name: "ix_department_key_metrics_key_metric_id",
                schema: "metrics",
                table: "department_key_metrics",
                column: "key_metric_id");

            migrationBuilder.CreateIndex(
                name: "ix_department_key_metrics_kpi_submission_period_id_department_",
                schema: "metrics",
                table: "department_key_metrics",
                columns: new[] { "kpi_submission_period_id", "department_id", "key_metric_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_key_metrics_metric_code",
                schema: "metrics",
                table: "key_metrics",
                column: "metric_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_key_metrics_metric_title",
                schema: "metrics",
                table: "key_metrics",
                column: "metric_title",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "fk_key_kpi_submission_items_department_key_metrics_key_kpi_met",
                schema: "metrics",
                table: "key_kpi_submission_items",
                column: "key_kpi_metrics_id",
                principalSchema: "metrics",
                principalTable: "department_key_metrics",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_key_kpi_submission_items_key_metrics_key_metric_id",
                schema: "metrics",
                table: "key_kpi_submission_items",
                column: "key_metric_id",
                principalSchema: "metrics",
                principalTable: "key_metrics",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_key_kpi_submissions_department_key_metrics_department_key_m",
                schema: "metrics",
                table: "key_kpi_submissions",
                column: "department_key_metric_id",
                principalSchema: "metrics",
                principalTable: "department_key_metrics",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_key_kpi_submissions_departments_department_id",
                schema: "metrics",
                table: "key_kpi_submissions",
                column: "department_id",
                principalSchema: "metrics",
                principalTable: "departments",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_key_kpi_submissions_key_metrics_key_metric_id",
                schema: "metrics",
                table: "key_kpi_submissions",
                column: "key_metric_id",
                principalSchema: "metrics",
                principalTable: "key_metrics",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_key_kpi_submission_items_department_key_metrics_key_kpi_met",
                schema: "metrics",
                table: "key_kpi_submission_items");

            migrationBuilder.DropForeignKey(
                name: "fk_key_kpi_submission_items_key_metrics_key_metric_id",
                schema: "metrics",
                table: "key_kpi_submission_items");

            migrationBuilder.DropForeignKey(
                name: "fk_key_kpi_submissions_department_key_metrics_department_key_m",
                schema: "metrics",
                table: "key_kpi_submissions");

            migrationBuilder.DropForeignKey(
                name: "fk_key_kpi_submissions_departments_department_id",
                schema: "metrics",
                table: "key_kpi_submissions");

            migrationBuilder.DropForeignKey(
                name: "fk_key_kpi_submissions_key_metrics_key_metric_id",
                schema: "metrics",
                table: "key_kpi_submissions");

            migrationBuilder.DropTable(
                name: "department_key_metrics",
                schema: "metrics");

            migrationBuilder.DropTable(
                name: "key_metrics",
                schema: "metrics");

            migrationBuilder.DropIndex(
                name: "ix_key_kpi_submissions_department_key_metric_id",
                schema: "metrics",
                table: "key_kpi_submissions");

            migrationBuilder.DropIndex(
                name: "ix_key_kpi_submissions_score_submission_period_id_department_i",
                schema: "metrics",
                table: "key_kpi_submissions");

            migrationBuilder.DropIndex(
                name: "ix_key_kpi_submission_items_key_kpi_submission_id_key_kpi_metr",
                schema: "metrics",
                table: "key_kpi_submission_items");

            migrationBuilder.DropIndex(
                name: "ix_key_kpi_submission_items_key_metric_id",
                schema: "metrics",
                table: "key_kpi_submission_items");

            migrationBuilder.DropColumn(
                name: "department_key_metric_id",
                schema: "metrics",
                table: "key_kpi_submissions");

            migrationBuilder.DropColumn(
                name: "key_metric_id",
                schema: "metrics",
                table: "key_kpi_submission_items");

            migrationBuilder.DropSequence(
                name: "department_key_metrics_id_seq",
                schema: "metrics");

            migrationBuilder.DropSequence(
                name: "key_metrics_id_seq",
                schema: "metrics");

            migrationBuilder.RenameColumn(
                name: "key_metric_id",
                schema: "metrics",
                table: "key_kpi_submissions",
                newName: "key_kpi_id");

            migrationBuilder.RenameIndex(
                name: "ix_key_kpi_submissions_key_metric_id",
                schema: "metrics",
                table: "key_kpi_submissions",
                newName: "ix_key_kpi_submissions_key_kpi_id");

            migrationBuilder.AlterDatabase()
                .OldAnnotation("Npgsql:PostgresExtension:citext", ",,");

            migrationBuilder.CreateSequence(
                name: "department_key_kpis_id_seq",
                schema: "metrics",
                incrementBy: 10);

            migrationBuilder.CreateSequence(
                name: "key_kpis_id_seq",
                schema: "metrics",
                incrementBy: 10);

            migrationBuilder.AlterColumn<string>(
                name: "title_name",
                schema: "metrics",
                table: "user_titles",
                type: "varchar (200)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "citext");

            migrationBuilder.AlterColumn<long>(
                name: "department_id",
                schema: "metrics",
                table: "key_kpi_submissions",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddColumn<long>(
                name: "department_id",
                schema: "metrics",
                table: "key_kpi_submission_items",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AlterColumn<string>(
                name: "department_name",
                schema: "metrics",
                table: "departments",
                type: "varchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "citext",
                oldMaxLength: 200);

            migrationBuilder.CreateTable(
                name: "key_kpis",
                schema: "metrics",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    metric_code = table.Column<Guid>(type: "uuid", nullable: false),
                    metric_title = table.Column<string>(type: "text", nullable: false),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_key_kpis", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "department_key_kpis",
                schema: "metrics",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    department_id = table.Column<long>(type: "bigint", nullable: false),
                    key_kpi_metric_id = table.Column<long>(type: "bigint", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_department_key_kpis", x => x.id);
                    table.ForeignKey(
                        name: "fk_department_key_kpis_departments_department_id",
                        column: x => x.department_id,
                        principalSchema: "metrics",
                        principalTable: "departments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_department_key_kpis_key_kpis_key_kpi_metric_id",
                        column: x => x.key_kpi_metric_id,
                        principalSchema: "metrics",
                        principalTable: "key_kpis",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_key_kpi_submissions_score_submission_period_id_application_",
                schema: "metrics",
                table: "key_kpi_submissions",
                columns: new[] { "score_submission_period_id", "application_user_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_key_kpi_submission_items_department_id",
                schema: "metrics",
                table: "key_kpi_submission_items",
                column: "department_id");

            migrationBuilder.CreateIndex(
                name: "ix_key_kpi_submission_items_key_kpi_submission_id_key_kpi_metr",
                schema: "metrics",
                table: "key_kpi_submission_items",
                columns: new[] { "key_kpi_submission_id", "key_kpi_metrics_id", "department_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_department_key_kpis_department_id",
                schema: "metrics",
                table: "department_key_kpis",
                column: "department_id");

            migrationBuilder.CreateIndex(
                name: "ix_department_key_kpis_key_kpi_metric_id",
                schema: "metrics",
                table: "department_key_kpis",
                column: "key_kpi_metric_id");

            migrationBuilder.CreateIndex(
                name: "ix_key_kpis_metric_code",
                schema: "metrics",
                table: "key_kpis",
                column: "metric_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_key_kpis_metric_title",
                schema: "metrics",
                table: "key_kpis",
                column: "metric_title",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "fk_key_kpi_submission_items_departments_department_id",
                schema: "metrics",
                table: "key_kpi_submission_items",
                column: "department_id",
                principalSchema: "metrics",
                principalTable: "departments",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_key_kpi_submission_items_key_kpis_key_kpi_metrics_id",
                schema: "metrics",
                table: "key_kpi_submission_items",
                column: "key_kpi_metrics_id",
                principalSchema: "metrics",
                principalTable: "key_kpis",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_key_kpi_submissions_departments_department_id",
                schema: "metrics",
                table: "key_kpi_submissions",
                column: "department_id",
                principalSchema: "metrics",
                principalTable: "departments",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_key_kpi_submissions_key_kpis_key_kpi_id",
                schema: "metrics",
                table: "key_kpi_submissions",
                column: "key_kpi_id",
                principalSchema: "metrics",
                principalTable: "key_kpis",
                principalColumn: "id");
        }
    }
}
