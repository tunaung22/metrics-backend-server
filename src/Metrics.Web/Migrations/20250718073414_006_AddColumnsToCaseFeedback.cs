using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Metrics.Web.Migrations
{
    /// <inheritdoc />
    public partial class _006_AddColumnsToCaseFeedback : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "kpi_submission_period_id",
                schema: "metrics",
                table: "case_feedback_submissions",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<Guid>(
                name: "lookup_id",
                schema: "metrics",
                table: "case_feedback_submissions",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "ix_key_kpi_submission_constraints_lookup_id",
                schema: "metrics",
                table: "key_kpi_submission_constraints",
                column: "lookup_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_department_key_metrics_department_key_metric_code",
                schema: "metrics",
                table: "department_key_metrics",
                column: "department_key_metric_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_case_feedback_submissions_kpi_submission_period_id",
                schema: "metrics",
                table: "case_feedback_submissions",
                column: "kpi_submission_period_id");

            migrationBuilder.CreateIndex(
                name: "ix_case_feedback_submissions_lookup_id",
                schema: "metrics",
                table: "case_feedback_submissions",
                column: "lookup_id",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "fk_case_feedback_submissions_kpi_submission_periods_kpi_submis",
                schema: "metrics",
                table: "case_feedback_submissions",
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
                name: "fk_case_feedback_submissions_kpi_submission_periods_kpi_submis",
                schema: "metrics",
                table: "case_feedback_submissions");

            migrationBuilder.DropIndex(
                name: "ix_key_kpi_submission_constraints_lookup_id",
                schema: "metrics",
                table: "key_kpi_submission_constraints");

            migrationBuilder.DropIndex(
                name: "ix_department_key_metrics_department_key_metric_code",
                schema: "metrics",
                table: "department_key_metrics");

            migrationBuilder.DropIndex(
                name: "ix_case_feedback_submissions_kpi_submission_period_id",
                schema: "metrics",
                table: "case_feedback_submissions");

            migrationBuilder.DropIndex(
                name: "ix_case_feedback_submissions_lookup_id",
                schema: "metrics",
                table: "case_feedback_submissions");

            migrationBuilder.DropColumn(
                name: "kpi_submission_period_id",
                schema: "metrics",
                table: "case_feedback_submissions");

            migrationBuilder.DropColumn(
                name: "lookup_id",
                schema: "metrics",
                table: "case_feedback_submissions");
        }
    }
}
