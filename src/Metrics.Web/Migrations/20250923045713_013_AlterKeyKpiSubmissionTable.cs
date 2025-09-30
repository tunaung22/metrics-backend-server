using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Metrics.Web.Migrations
{
    /// <inheritdoc />
    public partial class _013_AlterKeyKpiSubmissionTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_key_kpi_submissions_departments_department_id",
                schema: "metrics",
                table: "key_kpi_submissions");

            migrationBuilder.DropForeignKey(
                name: "fk_key_kpi_submissions_kpi_submission_periods_score_submission",
                schema: "metrics",
                table: "key_kpi_submissions");

            migrationBuilder.DropForeignKey(
                name: "fk_key_kpi_submissions_users_application_user_id",
                schema: "metrics",
                table: "key_kpi_submissions");

            migrationBuilder.DropTable(
                name: "key_kpi_submission_items",
                schema: "metrics");

            migrationBuilder.DropIndex(
                name: "ix_key_kpi_submissions_department_id",
                schema: "metrics",
                table: "key_kpi_submissions");

            migrationBuilder.DropIndex(
                name: "ix_key_kpi_submissions_period_id_dpt_id_user_id",
                schema: "metrics",
                table: "key_kpi_submissions");

            migrationBuilder.DropColumn(
                name: "department_id",
                schema: "metrics",
                table: "key_kpi_submissions");

            migrationBuilder.DropSequence(
                name: "key_kpi_submission_items_id_seq",
                schema: "metrics");

            migrationBuilder.RenameColumn(
                name: "score_submission_period_id",
                schema: "metrics",
                table: "key_kpi_submissions",
                newName: "department_key_metric_id");

            migrationBuilder.RenameColumn(
                name: "application_user_id",
                schema: "metrics",
                table: "key_kpi_submissions",
                newName: "submitter_id");

            migrationBuilder.RenameIndex(
                name: "ix_key_kpi_submissions_application_user_id",
                schema: "metrics",
                table: "key_kpi_submissions",
                newName: "ix_key_kpi_submissions_submitter_id");

            migrationBuilder.AddColumn<string>(
                name: "comments",
                schema: "metrics",
                table: "key_kpi_submissions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "score_value",
                schema: "metrics",
                table: "key_kpi_submissions",
                type: "numeric(4,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "ix_key_kpi_submissions_dkm_id_user_id",
                schema: "metrics",
                table: "key_kpi_submissions",
                columns: new[] { "department_key_metric_id", "submitter_id" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "fk_key_kpi_submissions_dkm_id",
                schema: "metrics",
                table: "key_kpi_submissions",
                column: "department_key_metric_id",
                principalSchema: "metrics",
                principalTable: "department_key_metrics",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_key_kpi_submissions_submitter_id",
                schema: "metrics",
                table: "key_kpi_submissions",
                column: "submitter_id",
                principalSchema: "metrics",
                principalTable: "application_users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_key_kpi_submissions_dkm_id",
                schema: "metrics",
                table: "key_kpi_submissions");

            migrationBuilder.DropForeignKey(
                name: "fk_key_kpi_submissions_submitter_id",
                schema: "metrics",
                table: "key_kpi_submissions");

            migrationBuilder.DropIndex(
                name: "ix_key_kpi_submissions_dkm_id_user_id",
                schema: "metrics",
                table: "key_kpi_submissions");

            migrationBuilder.DropColumn(
                name: "comments",
                schema: "metrics",
                table: "key_kpi_submissions");

            migrationBuilder.DropColumn(
                name: "score_value",
                schema: "metrics",
                table: "key_kpi_submissions");

            migrationBuilder.RenameColumn(
                name: "submitter_id",
                schema: "metrics",
                table: "key_kpi_submissions",
                newName: "application_user_id");

            migrationBuilder.RenameColumn(
                name: "department_key_metric_id",
                schema: "metrics",
                table: "key_kpi_submissions",
                newName: "score_submission_period_id");

            migrationBuilder.RenameIndex(
                name: "ix_key_kpi_submissions_submitter_id",
                schema: "metrics",
                table: "key_kpi_submissions",
                newName: "ix_key_kpi_submissions_application_user_id");

            migrationBuilder.CreateSequence(
                name: "key_kpi_submission_items_id_seq",
                schema: "metrics",
                incrementBy: 10);

            migrationBuilder.AddColumn<long>(
                name: "department_id",
                schema: "metrics",
                table: "key_kpi_submissions",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateTable(
                name: "key_kpi_submission_items",
                schema: "metrics",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    department_key_metric_id = table.Column<long>(type: "bigint", nullable: false),
                    key_kpi_submission_id = table.Column<long>(type: "bigint", nullable: false),
                    comments = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    score_value = table.Column<decimal>(type: "numeric(4,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_key_kpi_submission_items", x => x.id);
                    table.CheckConstraint("ck_kpi_submissions_kpi_score_gt_0", "score_value >= 0");
                    table.ForeignKey(
                        name: "fk_key_kpi_submission_items_department_key_metrics_department_",
                        column: x => x.department_key_metric_id,
                        principalSchema: "metrics",
                        principalTable: "department_key_metrics",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_key_kpi_submission_items_key_kpi_submissions_key_kpi_submis",
                        column: x => x.key_kpi_submission_id,
                        principalSchema: "metrics",
                        principalTable: "key_kpi_submissions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_key_kpi_submissions_department_id",
                schema: "metrics",
                table: "key_kpi_submissions",
                column: "department_id");

            migrationBuilder.CreateIndex(
                name: "ix_key_kpi_submissions_period_id_dpt_id_user_id",
                schema: "metrics",
                table: "key_kpi_submissions",
                columns: new[] { "score_submission_period_id", "department_id", "application_user_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_key_kpi_submission_items_department_key_metric_id",
                schema: "metrics",
                table: "key_kpi_submission_items",
                column: "department_key_metric_id");

            migrationBuilder.CreateIndex(
                name: "ix_key_kpi_submission_items_kks_id_dkm_id",
                schema: "metrics",
                table: "key_kpi_submission_items",
                columns: new[] { "key_kpi_submission_id", "department_key_metric_id" },
                unique: true);

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
                name: "fk_key_kpi_submissions_kpi_submission_periods_score_submission",
                schema: "metrics",
                table: "key_kpi_submissions",
                column: "score_submission_period_id",
                principalSchema: "metrics",
                principalTable: "kpi_submission_periods",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_key_kpi_submissions_users_application_user_id",
                schema: "metrics",
                table: "key_kpi_submissions",
                column: "application_user_id",
                principalSchema: "metrics",
                principalTable: "application_users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
