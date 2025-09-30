START TRANSACTION;
ALTER TABLE metrics.key_kpi_submission_items DROP CONSTRAINT fk_key_kpi_submission_items_key_metrics_key_metric_id;

ALTER TABLE metrics.key_kpi_submissions DROP CONSTRAINT fk_key_kpi_submissions_department_key_metrics_department_key_m;

ALTER TABLE metrics.key_kpi_submissions DROP CONSTRAINT fk_key_kpi_submissions_key_metrics_key_metric_id;

DROP INDEX metrics.ix_key_kpi_submissions_department_key_metric_id;

DROP INDEX metrics.ix_key_kpi_submissions_key_metric_id;

DROP INDEX metrics.ix_key_kpi_submission_items_key_metric_id;

ALTER TABLE metrics.key_kpi_submissions DROP COLUMN department_key_metric_id;

ALTER TABLE metrics.key_kpi_submissions DROP COLUMN key_metric_id;

ALTER TABLE metrics.key_kpi_submission_items DROP COLUMN key_metric_id;

INSERT INTO metrics.__ef_migrations_history (migration_id, product_version)
VALUES ('20250915111642_011_RemoveExtraColumns_FromKeyKpiSubmission', '9.0.2');

COMMIT;

