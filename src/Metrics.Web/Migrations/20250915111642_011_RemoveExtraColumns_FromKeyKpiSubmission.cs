using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Metrics.Web.Migrations
{
    /// <inheritdoc />
    public partial class _011_RemoveExtraColumns_FromKeyKpiSubmission : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_key_kpi_submission_items_key_metrics_key_metric_id",
                schema: "metrics",
                table: "key_kpi_submission_items");

            migrationBuilder.DropForeignKey(
                name: "fk_key_kpi_submissions_department_key_metrics_department_key_m",
                schema: "metrics",
                table: "key_kpi_submissions");

            migrationBuilder.DropForeignKey(
                name: "fk_key_kpi_submissions_key_metrics_key_metric_id",
                schema: "metrics",
                table: "key_kpi_submissions");

            migrationBuilder.DropIndex(
                name: "ix_key_kpi_submissions_department_key_metric_id",
                schema: "metrics",
                table: "key_kpi_submissions");

            migrationBuilder.DropIndex(
                name: "ix_key_kpi_submissions_key_metric_id",
                schema: "metrics",
                table: "key_kpi_submissions");

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
                table: "key_kpi_submissions");

            migrationBuilder.DropColumn(
                name: "key_metric_id",
                schema: "metrics",
                table: "key_kpi_submission_items");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "department_key_metric_id",
                schema: "metrics",
                table: "key_kpi_submissions",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "key_metric_id",
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

            migrationBuilder.CreateIndex(
                name: "ix_key_kpi_submissions_department_key_metric_id",
                schema: "metrics",
                table: "key_kpi_submissions",
                column: "department_key_metric_id");

            migrationBuilder.CreateIndex(
                name: "ix_key_kpi_submissions_key_metric_id",
                schema: "metrics",
                table: "key_kpi_submissions",
                column: "key_metric_id");

            migrationBuilder.CreateIndex(
                name: "ix_key_kpi_submission_items_key_metric_id",
                schema: "metrics",
                table: "key_kpi_submission_items",
                column: "key_metric_id");

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
                name: "fk_key_kpi_submissions_key_metrics_key_metric_id",
                schema: "metrics",
                table: "key_kpi_submissions",
                column: "key_metric_id",
                principalSchema: "metrics",
                principalTable: "key_metrics",
                principalColumn: "id");
        }
    }
}
