using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Metrics.Web.Migrations
{
    /// <inheritdoc />
    public partial class _012_MovePeriodColumnToFeedbackTableAndAddProceededColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_case_feedbacks_kpi_submission_periods_kpi_submission_period",
                schema: "metrics",
                table: "case_feedbacks");

            migrationBuilder.DropIndex(
                name: "ix_case_feedbacks_kpi_submission_period_id",
                schema: "metrics",
                table: "case_feedbacks");

            migrationBuilder.DropColumn(
                name: "kpi_submission_period_id",
                schema: "metrics",
                table: "case_feedbacks");

            migrationBuilder.AddColumn<bool>(
                name: "proceeded",
                schema: "metrics",
                table: "case_feedbacks",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<long>(
                name: "kpi_submission_period_id",
                schema: "metrics",
                table: "case_feedback_score_submissions",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<bool>(
                name: "proceeded",
                schema: "metrics",
                table: "case_feedback_score_submissions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "ix_case_feedback_score_submissions_kpi_submission_period_id",
                schema: "metrics",
                table: "case_feedback_score_submissions",
                column: "kpi_submission_period_id");

            migrationBuilder.AddForeignKey(
                name: "fk_casefeedbacksubmissions_period_id",
                schema: "metrics",
                table: "case_feedback_score_submissions",
                column: "kpi_submission_period_id",
                principalSchema: "metrics",
                principalTable: "kpi_submission_periods",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_casefeedbacksubmissions_period_id",
                schema: "metrics",
                table: "case_feedback_score_submissions");

            migrationBuilder.DropIndex(
                name: "ix_case_feedback_score_submissions_kpi_submission_period_id",
                schema: "metrics",
                table: "case_feedback_score_submissions");

            migrationBuilder.DropColumn(
                name: "proceeded",
                schema: "metrics",
                table: "case_feedbacks");

            migrationBuilder.DropColumn(
                name: "kpi_submission_period_id",
                schema: "metrics",
                table: "case_feedback_score_submissions");

            migrationBuilder.DropColumn(
                name: "proceeded",
                schema: "metrics",
                table: "case_feedback_score_submissions");

            migrationBuilder.AddColumn<long>(
                name: "kpi_submission_period_id",
                schema: "metrics",
                table: "case_feedbacks",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "ix_case_feedbacks_kpi_submission_period_id",
                schema: "metrics",
                table: "case_feedbacks",
                column: "kpi_submission_period_id");

            migrationBuilder.AddForeignKey(
                name: "fk_case_feedbacks_kpi_submission_periods_kpi_submission_period",
                schema: "metrics",
                table: "case_feedbacks",
                column: "kpi_submission_period_id",
                principalSchema: "metrics",
                principalTable: "kpi_submission_periods",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
