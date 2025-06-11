START TRANSACTION;
DROP INDEX metrics.ix_application_users_user_code;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM pg_namespace WHERE nspname = 'metrics') THEN
        CREATE SCHEMA metrics;
    END IF;
END $EF$;

CREATE EXTENSION IF NOT EXISTS citext;

ALTER TABLE metrics.user_titles ALTER COLUMN title_name TYPE citext;

ALTER TABLE metrics.departments ALTER COLUMN department_name TYPE citext;

CREATE UNIQUE INDEX ix_application_users_user_code ON metrics.application_users (user_code COLLATE "en_US.utf8");

INSERT INTO metrics.__ef_migrations_history (migration_id, product_version)
VALUES ('20250611175000_002_Addcitext', '9.0.2');

COMMIT;

