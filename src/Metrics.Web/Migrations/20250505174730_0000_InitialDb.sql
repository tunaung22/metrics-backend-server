DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM pg_namespace WHERE nspname = 'metrics') THEN
        CREATE SCHEMA metrics;
    END IF;
END $EF$;
CREATE TABLE IF NOT EXISTS metrics.__ef_migrations_history (
    migration_id character varying(150) NOT NULL,
    product_version character varying(32) NOT NULL,
    CONSTRAINT pk___ef_migrations_history PRIMARY KEY (migration_id)
);

START TRANSACTION;
DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM pg_namespace WHERE nspname = 'metrics') THEN
        CREATE SCHEMA metrics;
    END IF;
END $EF$;

CREATE SEQUENCE metrics."EntityFrameworkHiLoSequence" START WITH 1 INCREMENT BY 10 NO CYCLE;

CREATE TABLE metrics.asp_net_roles (
    id text NOT NULL,
    name character varying(256),
    normalized_name character varying(256),
    concurrency_stamp text,
    CONSTRAINT "PK_asp_net_roles" PRIMARY KEY (id)
);

CREATE TABLE metrics.asp_net_users (
    id text NOT NULL,
    user_name character varying(256),
    normalized_user_name character varying(256),
    email character varying(256),
    normalized_email character varying(256),
    email_confirmed boolean NOT NULL,
    password_hash text,
    security_stamp text,
    concurrency_stamp text,
    phone_number text,
    phone_number_confirmed boolean NOT NULL,
    two_factor_enabled boolean NOT NULL,
    lockout_end timestamp with time zone,
    lockout_enabled boolean NOT NULL,
    access_failed_count integer NOT NULL,
    CONSTRAINT "PK_asp_net_users" PRIMARY KEY (id)
);

CREATE TABLE metrics.departments (
    id bigint NOT NULL,
    department_code uuid NOT NULL,
    department_name varchar(200) NOT NULL,
    created_at timestamp with time zone NOT NULL,
    modified_at timestamp with time zone NOT NULL,
    CONSTRAINT pk_departments PRIMARY KEY (id)
);

CREATE TABLE metrics.kpi_periods (
    id bigint NOT NULL,
    period_code varchar(20) NOT NULL,
    submission_start_date timestamp with time zone NOT NULL,
    submission_end_date timestamp with time zone NOT NULL,
    created_at timestamp with time zone NOT NULL,
    modified_at timestamp with time zone NOT NULL,
    CONSTRAINT pk_kpi_periods PRIMARY KEY (id),
    CONSTRAINT ck_kpi_periods_is_correct_period_code_format CHECK (period_code ~ '^[0-9]{4}-[0-9]{2}$'),
    CONSTRAINT ck_kpi_periods_start_date_lt_end_date CHECK (submission_start_date < submission_end_date)
);

CREATE TABLE metrics.application_roles (
    id text NOT NULL,
    created_at timestamp with time zone NOT NULL,
    modified_at timestamp with time zone NOT NULL,
    CONSTRAINT "PK_application_roles" PRIMARY KEY (id),
    CONSTRAINT fk_application_roles_asp_net_roles_id FOREIGN KEY (id) REFERENCES metrics.asp_net_roles (id) ON DELETE CASCADE
);

CREATE TABLE metrics.asp_net_role_claims (
    id integer NOT NULL,
    role_id text NOT NULL,
    claim_type text,
    claim_value text,
    CONSTRAINT pk_asp_net_role_claims PRIMARY KEY (id),
    CONSTRAINT fk_asp_net_role_claims_asp_net_roles_role_id FOREIGN KEY (role_id) REFERENCES metrics.asp_net_roles (id) ON DELETE CASCADE
);

CREATE TABLE metrics.application_users (
    id text NOT NULL,
    created_at timestamp with time zone NOT NULL,
    modified_at timestamp with time zone NOT NULL,
    CONSTRAINT "PK_application_users" PRIMARY KEY (id),
    CONSTRAINT fk_application_users_asp_net_users_id FOREIGN KEY (id) REFERENCES metrics.asp_net_users (id) ON DELETE CASCADE
);

CREATE TABLE metrics.asp_net_user_claims (
    id integer NOT NULL,
    user_id text NOT NULL,
    claim_type text,
    claim_value text,
    CONSTRAINT pk_asp_net_user_claims PRIMARY KEY (id),
    CONSTRAINT fk_asp_net_user_claims_asp_net_users_user_id FOREIGN KEY (user_id) REFERENCES metrics.asp_net_users (id) ON DELETE CASCADE
);

CREATE TABLE metrics.asp_net_user_logins (
    login_provider text NOT NULL,
    provider_key text NOT NULL,
    provider_display_name text,
    user_id text NOT NULL,
    CONSTRAINT pk_asp_net_user_logins PRIMARY KEY (login_provider, provider_key),
    CONSTRAINT fk_asp_net_user_logins_asp_net_users_user_id FOREIGN KEY (user_id) REFERENCES metrics.asp_net_users (id) ON DELETE CASCADE
);

CREATE TABLE metrics.asp_net_user_roles (
    user_id text NOT NULL,
    role_id text NOT NULL,
    CONSTRAINT pk_asp_net_user_roles PRIMARY KEY (user_id, role_id),
    CONSTRAINT fk_asp_net_user_roles_asp_net_roles_role_id FOREIGN KEY (role_id) REFERENCES metrics.asp_net_roles (id) ON DELETE CASCADE,
    CONSTRAINT fk_asp_net_user_roles_asp_net_users_user_id FOREIGN KEY (user_id) REFERENCES metrics.asp_net_users (id) ON DELETE CASCADE
);

CREATE TABLE metrics.asp_net_user_tokens (
    user_id text NOT NULL,
    login_provider text NOT NULL,
    name text NOT NULL,
    value text,
    CONSTRAINT pk_asp_net_user_tokens PRIMARY KEY (user_id, login_provider, name),
    CONSTRAINT fk_asp_net_user_tokens_asp_net_users_user_id FOREIGN KEY (user_id) REFERENCES metrics.asp_net_users (id) ON DELETE CASCADE
);

CREATE TABLE metrics.employees (
    id bigint NOT NULL,
    employee_code varchar(200) NOT NULL,
    full_name varchar(200) NOT NULL,
    address text,
    phone_number varchar(200),
    created_at timestamp with time zone NOT NULL,
    modified_at timestamp with time zone NOT NULL,
    department_id bigint NOT NULL,
    application_user_id text NOT NULL,
    CONSTRAINT pk_employees PRIMARY KEY (id),
    CONSTRAINT fk_employees_application_users_application_user_id FOREIGN KEY (application_user_id) REFERENCES metrics.application_users (id) ON DELETE RESTRICT,
    CONSTRAINT fk_employees_departments_department_id FOREIGN KEY (department_id) REFERENCES metrics.departments (id) ON DELETE RESTRICT
);

CREATE TABLE metrics.kpi_submissions (
    id bigint NOT NULL,
    submitted_at timestamp with time zone NOT NULL,
    submission_date date GENERATED ALWAYS AS ((submitted_at AT TIME ZONE 'UTC')::date) STORED NOT NULL,
    kpi_score numeric(4,2) NOT NULL,
    comments text,
    created_at timestamp with time zone NOT NULL,
    modified_at timestamp with time zone NOT NULL,
    kpi_period_id bigint NOT NULL,
    department_id bigint NOT NULL,
    employee_id bigint NOT NULL,
    CONSTRAINT pk_kpi_submissions PRIMARY KEY (id),
    CONSTRAINT ck_kpi_submissions_kpi_score_gt_0 CHECK (kpi_score >= 0),
    CONSTRAINT fk_kpi_submissions_departments_department_id FOREIGN KEY (department_id) REFERENCES metrics.departments (id) ON DELETE RESTRICT,
    CONSTRAINT fk_kpi_submissions_employees_employee_id FOREIGN KEY (employee_id) REFERENCES metrics.employees (id) ON DELETE RESTRICT,
    CONSTRAINT fk_kpi_submissions_kpi_periods_kpi_period_id FOREIGN KEY (kpi_period_id) REFERENCES metrics.kpi_periods (id) ON DELETE RESTRICT
);

CREATE INDEX ix_asp_net_role_claims_role_id ON metrics.asp_net_role_claims (role_id);

CREATE UNIQUE INDEX "RoleNameIndex" ON metrics.asp_net_roles (normalized_name);

CREATE INDEX ix_asp_net_user_claims_user_id ON metrics.asp_net_user_claims (user_id);

CREATE INDEX ix_asp_net_user_logins_user_id ON metrics.asp_net_user_logins (user_id);

CREATE INDEX ix_asp_net_user_roles_role_id ON metrics.asp_net_user_roles (role_id);

CREATE INDEX "EmailIndex" ON metrics.asp_net_users (normalized_email);

CREATE UNIQUE INDEX "UserNameIndex" ON metrics.asp_net_users (normalized_user_name);

CREATE UNIQUE INDEX ix_departments_department_code ON metrics.departments (department_code);

CREATE UNIQUE INDEX ix_departments_department_name ON metrics.departments (department_name);

CREATE UNIQUE INDEX ix_employees_application_user_id ON metrics.employees (application_user_id);

CREATE INDEX ix_employees_department_id ON metrics.employees (department_id);

CREATE UNIQUE INDEX ix_employees_employee_code ON metrics.employees (employee_code);

CREATE UNIQUE INDEX ix_kpi_periods_period_code ON metrics.kpi_periods (period_code);

CREATE INDEX ix_kpi_submissions_department_id ON metrics.kpi_submissions (department_id);

CREATE INDEX ix_kpi_submissions_employee_id ON metrics.kpi_submissions (employee_id);

CREATE UNIQUE INDEX ix_kpi_submissions_kpi_period_id_department_id_employee_id ON metrics.kpi_submissions (kpi_period_id, department_id, employee_id);

INSERT INTO metrics.__ef_migrations_history (migration_id, product_version)
VALUES ('20250505174730_0000_InitialDb', '9.0.2');

COMMIT;

