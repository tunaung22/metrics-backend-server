using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Metrics.Web.Migrations
{
    /// <inheritdoc />
    public partial class _015_FixColumnIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_key_kpi_submission_constraints_department_key_metrics_depar",
                schema: "metrics",
                table: "key_kpi_submission_constraints");

            migrationBuilder.DropForeignKey(
                name: "fk_key_kpi_submission_constraints_departments_department_id",
                schema: "metrics",
                table: "key_kpi_submission_constraints");

            migrationBuilder.AddForeignKey(
                name: "fk_key_kpi_submission_constraints_candidate_dpt_id",
                schema: "metrics",
                table: "key_kpi_submission_constraints",
                column: "department_id",
                principalSchema: "metrics",
                principalTable: "departments",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_key_kpi_submission_constraints_dkm_id",
                schema: "metrics",
                table: "key_kpi_submission_constraints",
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
                name: "fk_key_kpi_submission_constraints_candidate_dpt_id",
                schema: "metrics",
                table: "key_kpi_submission_constraints");

            migrationBuilder.DropForeignKey(
                name: "fk_key_kpi_submission_constraints_dkm_id",
                schema: "metrics",
                table: "key_kpi_submission_constraints");

            migrationBuilder.AddForeignKey(
                name: "fk_key_kpi_submission_constraints_department_key_metrics_depar",
                schema: "metrics",
                table: "key_kpi_submission_constraints",
                column: "department_key_metric_id",
                principalSchema: "metrics",
                principalTable: "department_key_metrics",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_key_kpi_submission_constraints_departments_department_id",
                schema: "metrics",
                table: "key_kpi_submission_constraints",
                column: "department_id",
                principalSchema: "metrics",
                principalTable: "departments",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
