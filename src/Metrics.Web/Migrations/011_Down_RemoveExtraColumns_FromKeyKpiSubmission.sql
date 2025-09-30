START TRANSACTION;
ALTER TABLE metrics.key_kpi_submissions ADD department_key_metric_id bigint;

ALTER TABLE metrics.key_kpi_submissions ADD key_metric_id bigint;

ALTER TABLE metrics.key_kpi_submission_items ADD key_metric_id bigint;

CREATE INDEX ix_key_kpi_submissions_department_key_metric_id ON metrics.key_kpi_submissions (department_key_metric_id);

CREATE INDEX ix_key_kpi_submissions_key_metric_id ON metrics.key_kpi_submissions (key_metric_id);

CREATE INDEX ix_key_kpi_submission_items_key_metric_id ON metrics.key_kpi_submission_items (key_metric_id);

ALTER TABLE metrics.key_kpi_submission_items ADD CONSTRAINT fk_key_kpi_submission_items_key_metrics_key_metric_id FOREIGN KEY (key_metric_id) REFERENCES metrics.key_metrics (id);

ALTER TABLE metrics.key_kpi_submissions ADD CONSTRAINT fk_key_kpi_submissions_department_key_metrics_department_key_m FOREIGN KEY (department_key_metric_id) REFERENCES metrics.department_key_metrics (id);

ALTER TABLE metrics.key_kpi_submissions ADD CONSTRAINT fk_key_kpi_submissions_key_metrics_key_metric_id FOREIGN KEY (key_metric_id) REFERENCES metrics.key_metrics (id);

DELETE FROM metrics.__ef_migrations_history
WHERE migration_id = '20250915111642_011_RemoveExtraColumns_FromKeyKpiSubmission';

COMMIT;

