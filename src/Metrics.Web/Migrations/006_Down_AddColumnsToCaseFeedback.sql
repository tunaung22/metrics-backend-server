START TRANSACTION;
ALTER TABLE metrics.case_feedback_submissions DROP CONSTRAINT fk_case_feedback_submissions_kpi_submission_periods_kpi_submis;

DROP INDEX metrics.ix_key_kpi_submission_constraints_lookup_id;

DROP INDEX metrics.ix_department_key_metrics_department_key_metric_code;

DROP INDEX metrics.ix_case_feedback_submissions_kpi_submission_period_id;

DROP INDEX metrics.ix_case_feedback_submissions_lookup_id;

ALTER TABLE metrics.case_feedback_submissions DROP COLUMN kpi_submission_period_id;

ALTER TABLE metrics.case_feedback_submissions DROP COLUMN lookup_id;

DELETE FROM metrics.__ef_migrations_history
WHERE migration_id = '20250718073414_006_AddColumnsToCaseFeedback';

COMMIT;

