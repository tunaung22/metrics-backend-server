using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Metrics.Web.Migrations
{
    /// <inheritdoc />
    public partial class _009_AddUniqueIndex_FeedbackScoreSubmission : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_case_feedback_score_submissions_case_feedback_id",
                schema: "metrics",
                table: "case_feedback_score_submissions");

            migrationBuilder.CreateIndex(
                name: "ix_case_feedback_score_submissions_feedback_id_user_id",
                schema: "metrics",
                table: "case_feedback_score_submissions",
                columns: new[] { "case_feedback_id", "submitter_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_case_feedback_score_submissions_feedback_id_user_id",
                schema: "metrics",
                table: "case_feedback_score_submissions");

            migrationBuilder.CreateIndex(
                name: "ix_case_feedback_score_submissions_case_feedback_id",
                schema: "metrics",
                table: "case_feedback_score_submissions",
                column: "case_feedback_id");
        }
    }
}
