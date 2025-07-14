using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Metrics.Web.Migrations
{
    /// <inheritdoc />
    public partial class _003_FixKpiSubmissionItemsFK : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_key_kpi_submission_items_department_key_metrics_key_kpi_met",
                schema: "metrics",
                table: "key_kpi_submission_items");

            migrationBuilder.RenameColumn(
                name: "key_kpi_metrics_id",
                schema: "metrics",
                table: "key_kpi_submission_items",
                newName: "department_key_metric_id");

            migrationBuilder.RenameIndex(
                name: "ix_key_kpi_submission_items_key_kpi_submission_id_key_kpi_metr",
                schema: "metrics",
                table: "key_kpi_submission_items",
                newName: "ix_key_kpi_submission_items_key_kpi_submission_id_department_k");

            migrationBuilder.RenameIndex(
                name: "ix_key_kpi_submission_items_key_kpi_metrics_id",
                schema: "metrics",
                table: "key_kpi_submission_items",
                newName: "ix_key_kpi_submission_items_department_key_metric_id");

            migrationBuilder.AddForeignKey(
                name: "fk_key_kpi_submission_items_department_key_metrics_department_",
                schema: "metrics",
                table: "key_kpi_submission_items",
                column: "department_key_metric_id",
                principalSchema: "metrics",
                principalTable: "department_key_metrics",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_key_kpi_submission_items_department_key_metrics_department_",
                schema: "metrics",
                table: "key_kpi_submission_items");

            migrationBuilder.RenameColumn(
                name: "department_key_metric_id",
                schema: "metrics",
                table: "key_kpi_submission_items",
                newName: "key_kpi_metrics_id");

            migrationBuilder.RenameIndex(
                name: "ix_key_kpi_submission_items_key_kpi_submission_id_department_k",
                schema: "metrics",
                table: "key_kpi_submission_items",
                newName: "ix_key_kpi_submission_items_key_kpi_submission_id_key_kpi_metr");

            migrationBuilder.RenameIndex(
                name: "ix_key_kpi_submission_items_department_key_metric_id",
                schema: "metrics",
                table: "key_kpi_submission_items",
                newName: "ix_key_kpi_submission_items_key_kpi_metrics_id");

            migrationBuilder.AddForeignKey(
                name: "fk_key_kpi_submission_items_department_key_metrics_key_kpi_met",
                schema: "metrics",
                table: "key_kpi_submission_items",
                column: "key_kpi_metrics_id",
                principalSchema: "metrics",
                principalTable: "department_key_metrics",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
