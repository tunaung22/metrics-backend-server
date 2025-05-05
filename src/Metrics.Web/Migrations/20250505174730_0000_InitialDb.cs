using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Metrics.Web.Migrations
{
    /// <inheritdoc />
    public partial class _0000_InitialDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "metrics");

            migrationBuilder.CreateSequence(
                name: "EntityFrameworkHiLoSequence",
                schema: "metrics",
                incrementBy: 10);

            migrationBuilder.CreateTable(
                name: "asp_net_roles",
                schema: "metrics",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    normalized_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    concurrency_stamp = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_asp_net_roles", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "asp_net_users",
                schema: "metrics",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    user_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    normalized_user_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    normalized_email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    email_confirmed = table.Column<bool>(type: "boolean", nullable: false),
                    password_hash = table.Column<string>(type: "text", nullable: true),
                    security_stamp = table.Column<string>(type: "text", nullable: true),
                    concurrency_stamp = table.Column<string>(type: "text", nullable: true),
                    phone_number = table.Column<string>(type: "text", nullable: true),
                    phone_number_confirmed = table.Column<bool>(type: "boolean", nullable: false),
                    two_factor_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    lockout_end = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    lockout_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    access_failed_count = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_asp_net_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "departments",
                schema: "metrics",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    department_code = table.Column<Guid>(type: "uuid", nullable: false),
                    department_name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_departments", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "kpi_periods",
                schema: "metrics",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    period_code = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    submission_start_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    submission_end_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_kpi_periods", x => x.id);
                    table.CheckConstraint("ck_kpi_periods_is_correct_period_code_format", "period_code ~ '^[0-9]{4}-[0-9]{2}$'");
                    table.CheckConstraint("ck_kpi_periods_start_date_lt_end_date", "submission_start_date < submission_end_date");
                });

            migrationBuilder.CreateTable(
                name: "application_roles",
                schema: "metrics",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_application_roles", x => x.id);
                    table.ForeignKey(
                        name: "fk_application_roles_asp_net_roles_id",
                        column: x => x.id,
                        principalSchema: "metrics",
                        principalTable: "asp_net_roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "asp_net_role_claims",
                schema: "metrics",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    role_id = table.Column<string>(type: "text", nullable: false),
                    claim_type = table.Column<string>(type: "text", nullable: true),
                    claim_value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_asp_net_role_claims", x => x.id);
                    table.ForeignKey(
                        name: "fk_asp_net_role_claims_asp_net_roles_role_id",
                        column: x => x.role_id,
                        principalSchema: "metrics",
                        principalTable: "asp_net_roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "application_users",
                schema: "metrics",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_application_users", x => x.id);
                    table.ForeignKey(
                        name: "fk_application_users_asp_net_users_id",
                        column: x => x.id,
                        principalSchema: "metrics",
                        principalTable: "asp_net_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "asp_net_user_claims",
                schema: "metrics",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    user_id = table.Column<string>(type: "text", nullable: false),
                    claim_type = table.Column<string>(type: "text", nullable: true),
                    claim_value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_asp_net_user_claims", x => x.id);
                    table.ForeignKey(
                        name: "fk_asp_net_user_claims_asp_net_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "metrics",
                        principalTable: "asp_net_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "asp_net_user_logins",
                schema: "metrics",
                columns: table => new
                {
                    login_provider = table.Column<string>(type: "text", nullable: false),
                    provider_key = table.Column<string>(type: "text", nullable: false),
                    provider_display_name = table.Column<string>(type: "text", nullable: true),
                    user_id = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_asp_net_user_logins", x => new { x.login_provider, x.provider_key });
                    table.ForeignKey(
                        name: "fk_asp_net_user_logins_asp_net_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "metrics",
                        principalTable: "asp_net_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "asp_net_user_roles",
                schema: "metrics",
                columns: table => new
                {
                    user_id = table.Column<string>(type: "text", nullable: false),
                    role_id = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_asp_net_user_roles", x => new { x.user_id, x.role_id });
                    table.ForeignKey(
                        name: "fk_asp_net_user_roles_asp_net_roles_role_id",
                        column: x => x.role_id,
                        principalSchema: "metrics",
                        principalTable: "asp_net_roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_asp_net_user_roles_asp_net_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "metrics",
                        principalTable: "asp_net_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "asp_net_user_tokens",
                schema: "metrics",
                columns: table => new
                {
                    user_id = table.Column<string>(type: "text", nullable: false),
                    login_provider = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_asp_net_user_tokens", x => new { x.user_id, x.login_provider, x.name });
                    table.ForeignKey(
                        name: "fk_asp_net_user_tokens_asp_net_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "metrics",
                        principalTable: "asp_net_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "employees",
                schema: "metrics",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    employee_code = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    full_name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    address = table.Column<string>(type: "text", nullable: true),
                    phone_number = table.Column<string>(type: "varchar(200)", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    department_id = table.Column<long>(type: "bigint", nullable: false),
                    application_user_id = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_employees", x => x.id);
                    table.ForeignKey(
                        name: "fk_employees_application_users_application_user_id",
                        column: x => x.application_user_id,
                        principalSchema: "metrics",
                        principalTable: "application_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_employees_departments_department_id",
                        column: x => x.department_id,
                        principalSchema: "metrics",
                        principalTable: "departments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "kpi_submissions",
                schema: "metrics",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    submitted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    submission_date = table.Column<DateOnly>(type: "date", nullable: false, computedColumnSql: "(submitted_at AT TIME ZONE 'UTC')::date", stored: true),
                    kpi_score = table.Column<decimal>(type: "numeric(4,2)", nullable: false),
                    comments = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    kpi_period_id = table.Column<long>(type: "bigint", nullable: false),
                    department_id = table.Column<long>(type: "bigint", nullable: false),
                    employee_id = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_kpi_submissions", x => x.id);
                    table.CheckConstraint("ck_kpi_submissions_kpi_score_gt_0", "kpi_score >= 0");
                    table.ForeignKey(
                        name: "fk_kpi_submissions_departments_department_id",
                        column: x => x.department_id,
                        principalSchema: "metrics",
                        principalTable: "departments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_kpi_submissions_employees_employee_id",
                        column: x => x.employee_id,
                        principalSchema: "metrics",
                        principalTable: "employees",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_kpi_submissions_kpi_periods_kpi_period_id",
                        column: x => x.kpi_period_id,
                        principalSchema: "metrics",
                        principalTable: "kpi_periods",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_asp_net_role_claims_role_id",
                schema: "metrics",
                table: "asp_net_role_claims",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                schema: "metrics",
                table: "asp_net_roles",
                column: "normalized_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_asp_net_user_claims_user_id",
                schema: "metrics",
                table: "asp_net_user_claims",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_asp_net_user_logins_user_id",
                schema: "metrics",
                table: "asp_net_user_logins",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_asp_net_user_roles_role_id",
                schema: "metrics",
                table: "asp_net_user_roles",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                schema: "metrics",
                table: "asp_net_users",
                column: "normalized_email");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                schema: "metrics",
                table: "asp_net_users",
                column: "normalized_user_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_departments_department_code",
                schema: "metrics",
                table: "departments",
                column: "department_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_departments_department_name",
                schema: "metrics",
                table: "departments",
                column: "department_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_employees_application_user_id",
                schema: "metrics",
                table: "employees",
                column: "application_user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_employees_department_id",
                schema: "metrics",
                table: "employees",
                column: "department_id");

            migrationBuilder.CreateIndex(
                name: "ix_employees_employee_code",
                schema: "metrics",
                table: "employees",
                column: "employee_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_kpi_periods_period_code",
                schema: "metrics",
                table: "kpi_periods",
                column: "period_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_kpi_submissions_department_id",
                schema: "metrics",
                table: "kpi_submissions",
                column: "department_id");

            migrationBuilder.CreateIndex(
                name: "ix_kpi_submissions_employee_id",
                schema: "metrics",
                table: "kpi_submissions",
                column: "employee_id");

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
            migrationBuilder.DropTable(
                name: "application_roles",
                schema: "metrics");

            migrationBuilder.DropTable(
                name: "asp_net_role_claims",
                schema: "metrics");

            migrationBuilder.DropTable(
                name: "asp_net_user_claims",
                schema: "metrics");

            migrationBuilder.DropTable(
                name: "asp_net_user_logins",
                schema: "metrics");

            migrationBuilder.DropTable(
                name: "asp_net_user_roles",
                schema: "metrics");

            migrationBuilder.DropTable(
                name: "asp_net_user_tokens",
                schema: "metrics");

            migrationBuilder.DropTable(
                name: "kpi_submissions",
                schema: "metrics");

            migrationBuilder.DropTable(
                name: "asp_net_roles",
                schema: "metrics");

            migrationBuilder.DropTable(
                name: "employees",
                schema: "metrics");

            migrationBuilder.DropTable(
                name: "kpi_periods",
                schema: "metrics");

            migrationBuilder.DropTable(
                name: "application_users",
                schema: "metrics");

            migrationBuilder.DropTable(
                name: "departments",
                schema: "metrics");

            migrationBuilder.DropTable(
                name: "asp_net_users",
                schema: "metrics");

            migrationBuilder.DropSequence(
                name: "EntityFrameworkHiLoSequence",
                schema: "metrics");
        }
    }
}
