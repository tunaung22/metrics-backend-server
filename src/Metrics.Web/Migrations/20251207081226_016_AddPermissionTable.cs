using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Metrics.Web.Migrations
{
    /// <inheritdoc />
    public partial class _016_AddPermissionTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateSequence(
                name: "application_permissions_id_seq",
                schema: "metrics",
                incrementBy: 10);

            migrationBuilder.CreateTable(
                name: "application_permissions",
                schema: "metrics",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    last_modified_by_id = table.Column<string>(type: "text", nullable: false),
                    task_name = table.Column<string>(type: "citext", maxLength: 200, nullable: false),
                    user_department_id = table.Column<long>(type: "bigint", nullable: true),
                    user_group_id = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_application_permissions", x => x.id);
                    table.ForeignKey(
                        name: "fk_application_permissions_departments_user_department_id",
                        column: x => x.user_department_id,
                        principalSchema: "metrics",
                        principalTable: "departments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_application_permissions_user_titles_user_group_id",
                        column: x => x.user_group_id,
                        principalSchema: "metrics",
                        principalTable: "user_titles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_application_permissions_users_last_modified_by_id",
                        column: x => x.last_modified_by_id,
                        principalSchema: "metrics",
                        principalTable: "application_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_application_permissions_last_modified_by_id",
                schema: "metrics",
                table: "application_permissions",
                column: "last_modified_by_id");

            migrationBuilder.CreateIndex(
                name: "ix_application_permissions_task_name_department_group_id",
                schema: "metrics",
                table: "application_permissions",
                columns: new[] { "task_name", "user_department_id", "user_group_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_application_permissions_user_department_id",
                schema: "metrics",
                table: "application_permissions",
                column: "user_department_id");

            migrationBuilder.CreateIndex(
                name: "ix_application_permissions_user_group_id",
                schema: "metrics",
                table: "application_permissions",
                column: "user_group_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "application_permissions",
                schema: "metrics");

            migrationBuilder.DropSequence(
                name: "application_permissions_id_seq",
                schema: "metrics");
        }
    }
}
