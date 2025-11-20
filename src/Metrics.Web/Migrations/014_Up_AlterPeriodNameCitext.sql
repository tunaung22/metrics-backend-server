START TRANSACTION;
ALTER TABLE metrics.kpi_submission_periods ALTER COLUMN period_name TYPE citext;

INSERT INTO metrics.__ef_migrations_history (migration_id, product_version)
VALUES ('20251120104055_014_AlterPeriodNameCitext', '9.0.4');

COMMIT;

