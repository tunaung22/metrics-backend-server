using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Metrics.Web.Migrations
{
    /// <inheritdoc />
    public partial class _005_UpdateIndexNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "ix_user_titles_title_name",
                schema: "metrics",
                table: "user_titles",
                newName: "ix_user_titles_group_name");

            migrationBuilder.RenameIndex(
                name: "ix_user_titles_title_code",
                schema: "metrics",
                table: "user_titles",
                newName: "ix_user_titles_group_code");

            migrationBuilder.RenameIndex(
                name: "ix_kpi_submissions_kpi_submission_period_id_department_id_appl",
                schema: "metrics",
                table: "kpi_submissions",
                newName: "ix_kpi_submissions_period_id_dpt_id_user_id");

            migrationBuilder.RenameIndex(
                name: "ix_key_kpi_submissions_score_submission_period_id_department_i",
                schema: "metrics",
                table: "key_kpi_submissions",
                newName: "ix_key_kpi_submissions_period_id_dpt_id_user_id");

            migrationBuilder.RenameIndex(
                name: "ix_key_kpi_submission_items_key_kpi_submission_id_department_k",
                schema: "metrics",
                table: "key_kpi_submission_items",
                newName: "ix_key_kpi_submission_items_kks_id_dkm_id");

            migrationBuilder.RenameIndex(
                name: "ix_key_kpi_submission_constraints_department_id_department_key",
                schema: "metrics",
                table: "key_kpi_submission_constraints",
                newName: "ix_key_kpi_submission_constraints_dpt_id_dkm_id");

            migrationBuilder.RenameIndex(
                name: "ix_department_key_metrics_kpi_submission_period_id_department_",
                schema: "metrics",
                table: "department_key_metrics",
                newName: "ix_department_key_metrics_period_id_dpt_id_metric_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "ix_user_titles_group_name",
                schema: "metrics",
                table: "user_titles",
                newName: "ix_user_titles_title_name");

            migrationBuilder.RenameIndex(
                name: "ix_user_titles_group_code",
                schema: "metrics",
                table: "user_titles",
                newName: "ix_user_titles_title_code");

            migrationBuilder.RenameIndex(
                name: "ix_kpi_submissions_period_id_dpt_id_user_id",
                schema: "metrics",
                table: "kpi_submissions",
                newName: "ix_kpi_submissions_kpi_submission_period_id_department_id_appl");

            migrationBuilder.RenameIndex(
                name: "ix_key_kpi_submissions_period_id_dpt_id_user_id",
                schema: "metrics",
                table: "key_kpi_submissions",
                newName: "ix_key_kpi_submissions_score_submission_period_id_department_i");

            migrationBuilder.RenameIndex(
                name: "ix_key_kpi_submission_items_kks_id_dkm_id",
                schema: "metrics",
                table: "key_kpi_submission_items",
                newName: "ix_key_kpi_submission_items_key_kpi_submission_id_department_k");

            migrationBuilder.RenameIndex(
                name: "ix_key_kpi_submission_constraints_dpt_id_dkm_id",
                schema: "metrics",
                table: "key_kpi_submission_constraints",
                newName: "ix_key_kpi_submission_constraints_department_id_department_key");

            migrationBuilder.RenameIndex(
                name: "ix_department_key_metrics_period_id_dpt_id_metric_id",
                schema: "metrics",
                table: "department_key_metrics",
                newName: "ix_department_key_metrics_kpi_submission_period_id_department_");
        }
    }
}
