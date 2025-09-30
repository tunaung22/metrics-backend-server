START TRANSACTION;
ALTER TABLE metrics.user_titles ALTER COLUMN is_deleted SET DEFAULT FALSE;

ALTER TABLE metrics.kpi_submissions ADD is_deleted boolean NOT NULL DEFAULT FALSE;

ALTER TABLE metrics.kpi_submission_periods ADD is_deleted boolean NOT NULL DEFAULT FALSE;

ALTER TABLE metrics.key_kpi_submissions ADD is_deleted boolean NOT NULL DEFAULT FALSE;

ALTER TABLE metrics.key_kpi_submission_items ADD is_deleted boolean NOT NULL DEFAULT FALSE;

ALTER TABLE metrics.departments ALTER COLUMN is_deleted SET DEFAULT FALSE;

INSERT INTO metrics.__ef_migrations_history (migration_id, product_version)
VALUES ('20250903080527_010_AddSoftDeleteColumn', '9.0.2');

COMMIT;

