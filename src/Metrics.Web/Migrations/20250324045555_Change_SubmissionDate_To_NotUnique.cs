using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Metrics.Web.Migrations
{
    /// <inheritdoc />
    public partial class Change_SubmissionDate_To_NotUnique : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_kpi_submissions_kpi_period_id",
                schema: "metrics",
                table: "kpi_submissions");

            migrationBuilder.DropIndex(
                name: "ix_kpi_submissions_submission_date",
                schema: "metrics",
                table: "kpi_submissions");

            migrationBuilder.CreateIndex(
                name: "ix_kpi_submissions_kpi_period_id_department_id_employee_id",
                schema: "metrics",
                table: "kpi_submissions",
                columns: new[] { "kpi_period_id", "department_id", "employee_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_kpi_submissions_kpi_period_id_department_id_employee_id",
                schema: "metrics",
                table: "kpi_submissions");

            migrationBuilder.CreateIndex(
                name: "ix_kpi_submissions_kpi_period_id",
                schema: "metrics",
                table: "kpi_submissions",
                column: "kpi_period_id");

            migrationBuilder.CreateIndex(
                name: "ix_kpi_submissions_submission_date",
                schema: "metrics",
                table: "kpi_submissions",
                column: "submission_date",
                unique: true);
        }
    }
}
