using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Metrics.Web.Migrations
{
    /// <inheritdoc />
    public partial class _002_Addcitext : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_application_users_user_code",
                schema: "metrics",
                table: "application_users");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:citext", ",,");

            migrationBuilder.AlterColumn<string>(
                name: "title_name",
                schema: "metrics",
                table: "user_titles",
                type: "citext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar (200)");

            migrationBuilder.AlterColumn<string>(
                name: "department_name",
                schema: "metrics",
                table: "departments",
                type: "citext",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(200)",
                oldMaxLength: 200);

            migrationBuilder.CreateIndex(
                name: "ix_application_users_user_code",
                schema: "metrics",
                table: "application_users",
                column: "user_code",
                unique: true)
                .Annotation("Relational:Collation", new[] { "en_US.utf8" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_application_users_user_code",
                schema: "metrics",
                table: "application_users");

            migrationBuilder.AlterDatabase()
                .OldAnnotation("Npgsql:PostgresExtension:citext", ",,");

            migrationBuilder.AlterColumn<string>(
                name: "title_name",
                schema: "metrics",
                table: "user_titles",
                type: "varchar (200)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "citext");

            migrationBuilder.AlterColumn<string>(
                name: "department_name",
                schema: "metrics",
                table: "departments",
                type: "varchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "citext",
                oldMaxLength: 200);

            migrationBuilder.CreateIndex(
                name: "ix_application_users_user_code",
                schema: "metrics",
                table: "application_users",
                column: "user_code",
                unique: true);
        }
    }
}
