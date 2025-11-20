START TRANSACTION;
ALTER TABLE metrics.kpi_submission_periods ALTER COLUMN period_name TYPE varchar(20);

DELETE FROM metrics.__ef_migrations_history
WHERE migration_id = '20251120104055_014_AlterPeriodNameCitext';

COMMIT;

