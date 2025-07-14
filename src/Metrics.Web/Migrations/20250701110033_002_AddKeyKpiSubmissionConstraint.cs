using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Metrics.Web.Migrations
{
    /// <inheritdoc />
    public partial class _002_AddKeyKpiSubmissionConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateSequence(
                name: "key_kpi_submission_constraints_id_seq",
                schema: "metrics",
                incrementBy: 10);

            migrationBuilder.CreateTable(
                name: "key_kpi_submission_constraints",
                schema: "metrics",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    lookup_id = table.Column<Guid>(type: "uuid", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    department_id = table.Column<long>(type: "bigint", nullable: false),
                    department_key_metric_id = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_key_kpi_submission_constraints", x => x.id);
                    table.ForeignKey(
                        name: "fk_key_kpi_submission_constraints_department_key_metrics_depar",
                        column: x => x.department_key_metric_id,
                        principalSchema: "metrics",
                        principalTable: "department_key_metrics",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_key_kpi_submission_constraints_departments_department_id",
                        column: x => x.department_id,
                        principalSchema: "metrics",
                        principalTable: "departments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_key_kpi_submission_constraints_department_id_department_key",
                schema: "metrics",
                table: "key_kpi_submission_constraints",
                columns: new[] { "department_id", "department_key_metric_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_key_kpi_submission_constraints_department_key_metric_id",
                schema: "metrics",
                table: "key_kpi_submission_constraints",
                column: "department_key_metric_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "key_kpi_submission_constraints",
                schema: "metrics");

            migrationBuilder.DropSequence(
                name: "key_kpi_submission_constraints_id_seq",
                schema: "metrics");
        }
    }
}
