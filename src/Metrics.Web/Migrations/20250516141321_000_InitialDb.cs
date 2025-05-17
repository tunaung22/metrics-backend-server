using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Metrics.Web.Migrations
{
    /// <inheritdoc />
    public partial class _000_InitialDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "metrics");

            migrationBuilder.CreateSequence(
                name: "department_key_kpis_id_seq",
                schema: "metrics",
                incrementBy: 10);

            migrationBuilder.CreateSequence(
                name: "departments_id_seq",
                schema: "metrics",
                incrementBy: 10);

            migrationBuilder.CreateSequence(
                name: "key_kpi_submission_items_id_seq",
                schema: "metrics",
                incrementBy: 10);

            migrationBuilder.CreateSequence(
                name: "key_kpi_submissions_id_seq",
                schema: "metrics",
                incrementBy: 10);

            migrationBuilder.CreateSequence(
                name: "key_kpis_id_seq",
                schema: "metrics",
                incrementBy: 10);

            migrationBuilder.CreateSequence(
                name: "kpi_submission_periods_id_seq",
                schema: "metrics",
                incrementBy: 10);

            migrationBuilder.CreateSequence(
                name: "kpi_submissions_id_seq",
                schema: "metrics",
                incrementBy: 10);

            migrationBuilder.CreateTable(
                name: "application_roles",
                schema: "metrics",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    normalized_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    concurrency_stamp = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_application_roles", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "departments",
                schema: "metrics",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    department_code = table.Column<Guid>(type: "uuid", nullable: false),
                    department_name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_departments", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "key_kpis",
                schema: "metrics",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    metric_code = table.Column<Guid>(type: "uuid", nullable: false),
                    metric_title = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_key_kpis", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "kpi_submission_periods",
                schema: "metrics",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    period_name = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    submission_start_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    submission_end_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_kpi_submission_periods", x => x.id);
                    table.CheckConstraint("ck_kpi_submission_periods_is_correct_period_code_format", "period_name ~ '^[0-9]{4}-[0-9]{2}$'");
                    table.CheckConstraint("ck_kpi_submission_periods_start_date_lt_end_date", "submission_start_date < submission_end_date");
                });

            migrationBuilder.CreateTable(
                name: "asp_net_role_claims",
                schema: "metrics",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
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
                        principalTable: "application_roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "application_users",
                schema: "metrics",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    user_code = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    full_name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    contact_address = table.Column<string>(type: "varchar(200)", nullable: false),
                    profile_picture_url = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    department_id = table.Column<long>(type: "bigint", nullable: false),
                    user_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    normalized_user_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    normalized_email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    email_confirmed = table.Column<bool>(type: "boolean", nullable: false),
                    password_hash = table.Column<string>(type: "text", nullable: true),
                    security_stamp = table.Column<string>(type: "text", nullable: true),
                    concurrency_stamp = table.Column<string>(type: "text", nullable: true),
                    phone_number = table.Column<string>(type: "varchar(200)", nullable: true),
                    phone_number_confirmed = table.Column<bool>(type: "boolean", nullable: false),
                    two_factor_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    lockout_end = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    lockout_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    access_failed_count = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_application_users", x => x.id);
                    table.ForeignKey(
                        name: "fk_application_users_departments_department_id",
                        column: x => x.department_id,
                        principalSchema: "metrics",
                        principalTable: "departments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "department_key_kpis",
                schema: "metrics",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    department_id = table.Column<long>(type: "bigint", nullable: false),
                    key_kpi_metric_id = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_department_key_kpis", x => x.id);
                    table.ForeignKey(
                        name: "fk_department_key_kpis_departments_department_id",
                        column: x => x.department_id,
                        principalSchema: "metrics",
                        principalTable: "departments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_department_key_kpis_key_kpis_key_kpi_metric_id",
                        column: x => x.key_kpi_metric_id,
                        principalSchema: "metrics",
                        principalTable: "key_kpis",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "asp_net_user_claims",
                schema: "metrics",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
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
                        principalTable: "application_users",
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
                        principalTable: "application_users",
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
                        principalTable: "application_roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_asp_net_user_roles_asp_net_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "metrics",
                        principalTable: "application_users",
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
                        principalTable: "application_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "key_kpi_submissions",
                schema: "metrics",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    submitted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    submission_date = table.Column<DateOnly>(type: "date", nullable: false, computedColumnSql: "(submitted_at AT TIME ZONE 'UTC')::date", stored: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    score_submission_period_id = table.Column<long>(type: "bigint", nullable: false),
                    application_user_id = table.Column<string>(type: "text", nullable: false),
                    department_id = table.Column<long>(type: "bigint", nullable: true),
                    key_kpi_id = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_key_kpi_submissions", x => x.id);
                    table.ForeignKey(
                        name: "fk_key_kpi_submissions_departments_department_id",
                        column: x => x.department_id,
                        principalSchema: "metrics",
                        principalTable: "departments",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_key_kpi_submissions_key_kpis_key_kpi_id",
                        column: x => x.key_kpi_id,
                        principalSchema: "metrics",
                        principalTable: "key_kpis",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_key_kpi_submissions_kpi_submission_periods_score_submission",
                        column: x => x.score_submission_period_id,
                        principalSchema: "metrics",
                        principalTable: "kpi_submission_periods",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_key_kpi_submissions_users_application_user_id",
                        column: x => x.application_user_id,
                        principalSchema: "metrics",
                        principalTable: "application_users",
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
                    score_value = table.Column<decimal>(type: "numeric(6,2)", maxLength: 100, nullable: false),
                    positive_aspects = table.Column<string>(type: "text", nullable: true),
                    negative_aspects = table.Column<string>(type: "text", nullable: true),
                    comments = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    kpi_submission_period_id = table.Column<long>(type: "bigint", nullable: false),
                    department_id = table.Column<long>(type: "bigint", nullable: false),
                    application_user_id = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_kpi_submissions", x => x.id);
                    table.CheckConstraint("ck_kpi_submissions_kpi_score_gt_0", "score_value >= 0");
                    table.ForeignKey(
                        name: "fk_kpi_submissions_departments_department_id",
                        column: x => x.department_id,
                        principalSchema: "metrics",
                        principalTable: "departments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_kpi_submissions_kpi_submission_periods_kpi_submission_perio",
                        column: x => x.kpi_submission_period_id,
                        principalSchema: "metrics",
                        principalTable: "kpi_submission_periods",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_kpi_submissions_users_application_user_id",
                        column: x => x.application_user_id,
                        principalSchema: "metrics",
                        principalTable: "application_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "key_kpi_submission_items",
                schema: "metrics",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    score_value = table.Column<decimal>(type: "numeric(4,2)", nullable: false),
                    comments = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    key_kpi_submission_id = table.Column<long>(type: "bigint", nullable: false),
                    department_id = table.Column<long>(type: "bigint", nullable: false),
                    key_kpi_metrics_id = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_key_kpi_submission_items", x => x.id);
                    table.CheckConstraint("ck_kpi_submissions_kpi_score_gt_0", "score_value >= 0");
                    table.ForeignKey(
                        name: "fk_key_kpi_submission_items_departments_department_id",
                        column: x => x.department_id,
                        principalSchema: "metrics",
                        principalTable: "departments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_key_kpi_submission_items_key_kpi_submissions_key_kpi_submis",
                        column: x => x.key_kpi_submission_id,
                        principalSchema: "metrics",
                        principalTable: "key_kpi_submissions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_key_kpi_submission_items_key_kpis_key_kpi_metrics_id",
                        column: x => x.key_kpi_metrics_id,
                        principalSchema: "metrics",
                        principalTable: "key_kpis",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                schema: "metrics",
                table: "application_roles",
                column: "normalized_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                schema: "metrics",
                table: "application_users",
                column: "normalized_email");

            migrationBuilder.CreateIndex(
                name: "ix_application_users_department_id",
                schema: "metrics",
                table: "application_users",
                column: "department_id");

            migrationBuilder.CreateIndex(
                name: "ix_application_users_user_code",
                schema: "metrics",
                table: "application_users",
                column: "user_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                schema: "metrics",
                table: "application_users",
                column: "normalized_user_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_asp_net_role_claims_role_id",
                schema: "metrics",
                table: "asp_net_role_claims",
                column: "role_id");

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
                name: "ix_department_key_kpis_department_id",
                schema: "metrics",
                table: "department_key_kpis",
                column: "department_id");

            migrationBuilder.CreateIndex(
                name: "ix_department_key_kpis_key_kpi_metric_id",
                schema: "metrics",
                table: "department_key_kpis",
                column: "key_kpi_metric_id");

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
                name: "ix_key_kpi_submission_items_department_id",
                schema: "metrics",
                table: "key_kpi_submission_items",
                column: "department_id");

            migrationBuilder.CreateIndex(
                name: "ix_key_kpi_submission_items_key_kpi_metrics_id",
                schema: "metrics",
                table: "key_kpi_submission_items",
                column: "key_kpi_metrics_id");

            migrationBuilder.CreateIndex(
                name: "ix_key_kpi_submission_items_key_kpi_submission_id_key_kpi_metr",
                schema: "metrics",
                table: "key_kpi_submission_items",
                columns: new[] { "key_kpi_submission_id", "key_kpi_metrics_id", "department_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_key_kpi_submissions_application_user_id",
                schema: "metrics",
                table: "key_kpi_submissions",
                column: "application_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_key_kpi_submissions_department_id",
                schema: "metrics",
                table: "key_kpi_submissions",
                column: "department_id");

            migrationBuilder.CreateIndex(
                name: "ix_key_kpi_submissions_key_kpi_id",
                schema: "metrics",
                table: "key_kpi_submissions",
                column: "key_kpi_id");

            migrationBuilder.CreateIndex(
                name: "ix_key_kpi_submissions_score_submission_period_id_application_",
                schema: "metrics",
                table: "key_kpi_submissions",
                columns: new[] { "score_submission_period_id", "application_user_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_key_kpis_metric_code",
                schema: "metrics",
                table: "key_kpis",
                column: "metric_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_key_kpis_metric_title",
                schema: "metrics",
                table: "key_kpis",
                column: "metric_title",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_kpi_submission_periods_period_name",
                schema: "metrics",
                table: "kpi_submission_periods",
                column: "period_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_kpi_submissions_application_user_id",
                schema: "metrics",
                table: "kpi_submissions",
                column: "application_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_kpi_submissions_department_id",
                schema: "metrics",
                table: "kpi_submissions",
                column: "department_id");

            migrationBuilder.CreateIndex(
                name: "ix_kpi_submissions_kpi_submission_period_id_department_id_appl",
                schema: "metrics",
                table: "kpi_submissions",
                columns: new[] { "kpi_submission_period_id", "department_id", "application_user_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
                name: "department_key_kpis",
                schema: "metrics");

            migrationBuilder.DropTable(
                name: "key_kpi_submission_items",
                schema: "metrics");

            migrationBuilder.DropTable(
                name: "kpi_submissions",
                schema: "metrics");

            migrationBuilder.DropTable(
                name: "application_roles",
                schema: "metrics");

            migrationBuilder.DropTable(
                name: "key_kpi_submissions",
                schema: "metrics");

            migrationBuilder.DropTable(
                name: "key_kpis",
                schema: "metrics");

            migrationBuilder.DropTable(
                name: "kpi_submission_periods",
                schema: "metrics");

            migrationBuilder.DropTable(
                name: "application_users",
                schema: "metrics");

            migrationBuilder.DropTable(
                name: "departments",
                schema: "metrics");

            migrationBuilder.DropSequence(
                name: "department_key_kpis_id_seq",
                schema: "metrics");

            migrationBuilder.DropSequence(
                name: "departments_id_seq",
                schema: "metrics");

            migrationBuilder.DropSequence(
                name: "key_kpi_submission_items_id_seq",
                schema: "metrics");

            migrationBuilder.DropSequence(
                name: "key_kpi_submissions_id_seq",
                schema: "metrics");

            migrationBuilder.DropSequence(
                name: "key_kpis_id_seq",
                schema: "metrics");

            migrationBuilder.DropSequence(
                name: "kpi_submission_periods_id_seq",
                schema: "metrics");

            migrationBuilder.DropSequence(
                name: "kpi_submissions_id_seq",
                schema: "metrics");
        }
    }
}
