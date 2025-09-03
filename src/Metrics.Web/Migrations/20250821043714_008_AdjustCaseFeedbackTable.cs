using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Metrics.Web.Migrations
{
    /// <inheritdoc />
    public partial class _008_AdjustCaseFeedbackTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "case_feedback_submissions",
                schema: "metrics");

            migrationBuilder.DropSequence(
                name: "case_feedback_submissions_id_seq",
                schema: "metrics");

            migrationBuilder.CreateSequence(
                name: "case_feedback_score_submissions_id_seq",
                schema: "metrics",
                incrementBy: 10);

            migrationBuilder.CreateSequence(
                name: "case_feedbacks_id_seq",
                schema: "metrics",
                incrementBy: 10);

            migrationBuilder.CreateTable(
                name: "case_feedbacks",
                schema: "metrics",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    lookup_id = table.Column<Guid>(type: "uuid", nullable: false),
                    kpi_submission_period_id = table.Column<long>(type: "bigint", nullable: false),
                    submitted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    submission_date = table.Column<DateOnly>(type: "date", nullable: false, computedColumnSql: "(submitted_at AT TIME ZONE 'UTC')::date", stored: true),
                    submitter_id = table.Column<string>(type: "text", nullable: false),
                    case_department_id = table.Column<long>(type: "bigint", nullable: false),
                    ward_name = table.Column<string>(type: "citext", maxLength: 200, nullable: false),
                    cpi_number = table.Column<string>(type: "citext", maxLength: 200, nullable: false),
                    patient_name = table.Column<string>(type: "citext", maxLength: 200, nullable: false),
                    room_number = table.Column<string>(type: "citext", maxLength: 200, nullable: false),
                    incident_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_case_feedbacks", x => x.id);
                    table.ForeignKey(
                        name: "fk_case_feedbacks_application_users_submitter_id",
                        column: x => x.submitter_id,
                        principalSchema: "metrics",
                        principalTable: "application_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_case_feedbacks_departments_case_department_id",
                        column: x => x.case_department_id,
                        principalSchema: "metrics",
                        principalTable: "departments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_case_feedbacks_kpi_submission_periods_kpi_submission_period",
                        column: x => x.kpi_submission_period_id,
                        principalSchema: "metrics",
                        principalTable: "kpi_submission_periods",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "case_feedback_score_submissions",
                schema: "metrics",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    lookup_id = table.Column<Guid>(type: "uuid", nullable: false),
                    submitted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    submission_date = table.Column<DateOnly>(type: "date", nullable: false, computedColumnSql: "(submitted_at AT TIME ZONE 'UTC')::date", stored: true),
                    negative_score_value = table.Column<decimal>(type: "numeric(4,2)", nullable: false),
                    comments = table.Column<string>(type: "text", nullable: true),
                    submitter_id = table.Column<string>(type: "text", nullable: false),
                    case_feedback_id = table.Column<long>(type: "bigint", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_case_feedback_score_submissions", x => x.id);
                    table.ForeignKey(
                        name: "fk_case_feedback_score_submissions_application_users_submitter",
                        column: x => x.submitter_id,
                        principalSchema: "metrics",
                        principalTable: "application_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_casefeedbacksubmissions_casefeedbacks_case_feedback_id",
                        column: x => x.case_feedback_id,
                        principalSchema: "metrics",
                        principalTable: "case_feedbacks",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_case_feedback_score_submissions_case_feedback_id",
                schema: "metrics",
                table: "case_feedback_score_submissions",
                column: "case_feedback_id");

            migrationBuilder.CreateIndex(
                name: "ix_case_feedback_score_submissions_lookup_id",
                schema: "metrics",
                table: "case_feedback_score_submissions",
                column: "lookup_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_case_feedback_score_submissions_submitter_id",
                schema: "metrics",
                table: "case_feedback_score_submissions",
                column: "submitter_id");

            migrationBuilder.CreateIndex(
                name: "ix_case_feedbacks_case_department_id",
                schema: "metrics",
                table: "case_feedbacks",
                column: "case_department_id");

            migrationBuilder.CreateIndex(
                name: "ix_case_feedbacks_kpi_submission_period_id",
                schema: "metrics",
                table: "case_feedbacks",
                column: "kpi_submission_period_id");

            migrationBuilder.CreateIndex(
                name: "ix_case_feedbacks_lookup_id",
                schema: "metrics",
                table: "case_feedbacks",
                column: "lookup_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_case_feedbacks_submitter_id",
                schema: "metrics",
                table: "case_feedbacks",
                column: "submitter_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "case_feedback_score_submissions",
                schema: "metrics");

            migrationBuilder.DropTable(
                name: "case_feedbacks",
                schema: "metrics");

            migrationBuilder.DropSequence(
                name: "case_feedback_score_submissions_id_seq",
                schema: "metrics");

            migrationBuilder.DropSequence(
                name: "case_feedbacks_id_seq",
                schema: "metrics");

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
                    case_department_id = table.Column<long>(type: "bigint", nullable: false),
                    kpi_submission_period_id = table.Column<long>(type: "bigint", nullable: false),
                    submitter_id = table.Column<string>(type: "text", nullable: false),
                    cpi_number = table.Column<string>(type: "citext", maxLength: 200, nullable: false),
                    comments = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    incident_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    lookup_id = table.Column<Guid>(type: "uuid", nullable: false),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    negative_score_value = table.Column<decimal>(type: "numeric(4,2)", nullable: false),
                    patient_name = table.Column<string>(type: "citext", maxLength: 200, nullable: false),
                    room_number = table.Column<string>(type: "citext", maxLength: 200, nullable: false),
                    submission_date = table.Column<DateOnly>(type: "date", nullable: false, computedColumnSql: "(submitted_at AT TIME ZONE 'UTC')::date", stored: true),
                    submitted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ward_name = table.Column<string>(type: "citext", maxLength: 200, nullable: false)
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
                    table.ForeignKey(
                        name: "fk_case_feedback_submissions_kpi_submission_periods_kpi_submis",
                        column: x => x.kpi_submission_period_id,
                        principalSchema: "metrics",
                        principalTable: "kpi_submission_periods",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_case_feedback_submissions_case_department_id",
                schema: "metrics",
                table: "case_feedback_submissions",
                column: "case_department_id");

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

            migrationBuilder.CreateIndex(
                name: "ix_case_feedback_submissions_submitter_id",
                schema: "metrics",
                table: "case_feedback_submissions",
                column: "submitter_id");
        }
    }
}
