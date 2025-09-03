using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Metrics.Web.Migrations
{
    /// <inheritdoc />
    public partial class _010_AddSoftDeleteColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<bool>(
                name: "is_deleted",
                schema: "metrics",
                table: "user_titles",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                schema: "metrics",
                table: "kpi_submissions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                schema: "metrics",
                table: "kpi_submission_periods",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                schema: "metrics",
                table: "key_kpi_submissions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                schema: "metrics",
                table: "key_kpi_submission_items",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "is_deleted",
                schema: "metrics",
                table: "departments",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_deleted",
                schema: "metrics",
                table: "kpi_submissions");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                schema: "metrics",
                table: "kpi_submission_periods");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                schema: "metrics",
                table: "key_kpi_submissions");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                schema: "metrics",
                table: "key_kpi_submission_items");

            migrationBuilder.AlterColumn<bool>(
                name: "is_deleted",
                schema: "metrics",
                table: "user_titles",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "is_deleted",
                schema: "metrics",
                table: "departments",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false);
        }
    }
}
