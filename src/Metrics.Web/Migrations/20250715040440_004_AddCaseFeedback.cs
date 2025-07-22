using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Metrics.Web.Migrations
{
    /// <inheritdoc />
    public partial class _004_AddCaseFeedback : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateSequence(
                name: "case_feedback_submissions_id_seq",
                schema: "metrics",
                incrementBy: 10);

            migrationBuilder.CreateTable(
                name: "case_feedback_submissions",
                schema: "metrics",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    submitted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    submission_date = table.Column<DateOnly>(type: "date", nullable: false, computedColumnSql: "(submitted_at AT TIME ZONE 'UTC')::date", stored: true),
                    negative_score_value = table.Column<decimal>(type: "numeric(4,2)", nullable: false),
                    submitter_id = table.Column<string>(type: "text", nullable: false),
                    case_department_id = table.Column<long>(type: "bigint", nullable: false),
                    ward_name = table.Column<string>(type: "citext", maxLength: 200, nullable: false),
                    cpi_number = table.Column<string>(type: "citext", maxLength: 200, nullable: false),
                    patient_name = table.Column<string>(type: "citext", maxLength: 200, nullable: false),
                    room_number = table.Column<string>(type: "citext", maxLength: 200, nullable: false),
                    incident_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    comments = table.Column<string>(type: "text", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_case_feedback_submissions", x => x.id);
                    table.ForeignKey(
                        name: "fk_case_feedback_submissions_application_users_submitter_id",
                        column: x => x.submitter_id,
                        principalSchema: "metrics",
                        principalTable: "application_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_case_feedback_submissions_departments_case_department_id",
                        column: x => x.case_department_id,
                        principalSchema: "metrics",
                        principalTable: "departments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_case_feedback_submissions_case_department_id",
                schema: "metrics",
                table: "case_feedback_submissions",
                column: "case_department_id");

            migrationBuilder.CreateIndex(
                name: "ix_case_feedback_submissions_submitter_id",
                schema: "metrics",
                table: "case_feedback_submissions",
                column: "submitter_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "case_feedback_submissions",
                schema: "metrics");

            migrationBuilder.DropSequence(
                name: "case_feedback_submissions_id_seq",
                schema: "metrics");
        }
    }
}
