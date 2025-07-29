START TRANSACTION;
ALTER TABLE metrics.application_users DROP COLUMN is_password_change_required;

DELETE FROM metrics.__ef_migrations_history
WHERE migration_id = '20250729170257_007_AddChangePasswordFlag';

COMMIT;

