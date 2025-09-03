START TRANSACTION;
ALTER TABLE metrics.kpi_submissions DROP COLUMN is_deleted;

ALTER TABLE metrics.kpi_submission_periods DROP COLUMN is_deleted;

ALTER TABLE metrics.key_kpi_submissions DROP COLUMN is_deleted;

ALTER TABLE metrics.key_kpi_submission_items DROP COLUMN is_deleted;

ALTER TABLE metrics.user_titles ALTER COLUMN is_deleted DROP DEFAULT;

ALTER TABLE metrics.departments ALTER COLUMN is_deleted DROP DEFAULT;

DELETE FROM metrics.__ef_migrations_history
WHERE migration_id = '20250903080527_010_AddSoftDeleteColumn';

COMMIT;

