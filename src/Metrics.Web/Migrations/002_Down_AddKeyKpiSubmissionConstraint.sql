START TRANSACTION;
DROP TABLE metrics.key_kpi_submission_constraints;

DROP SEQUENCE metrics.key_kpi_submission_constraints_id_seq;

DELETE FROM metrics.__ef_migrations_history
WHERE migration_id = '20250701110033_002_AddKeyKpiSubmissionConstraint';

COMMIT;

