START TRANSACTION;
DROP INDEX metrics.ix_application_users_user_code;

ALTER TABLE metrics.user_titles ALTER COLUMN title_name TYPE varchar (200);

ALTER TABLE metrics.departments ALTER COLUMN department_name TYPE varchar(200);

CREATE UNIQUE INDEX ix_application_users_user_code ON metrics.application_users (user_code);

DELETE FROM metrics.__ef_migrations_history
WHERE migration_id = '20250611175000_002_Addcitext';

COMMIT;

