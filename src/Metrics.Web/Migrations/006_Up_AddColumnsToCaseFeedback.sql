START TRANSACTION;
ALTER TABLE metrics.case_feedback_submissions ADD kpi_submission_period_id bigint NOT NULL DEFAULT 0;

ALTER TABLE metrics.case_feedback_submissions ADD lookup_id uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';

CREATE UNIQUE INDEX ix_key_kpi_submission_constraints_lookup_id ON metrics.key_kpi_submission_constraints (lookup_id);

CREATE UNIQUE INDEX ix_department_key_metrics_department_key_metric_code ON metrics.department_key_metrics (department_key_metric_code);

CREATE INDEX ix_case_feedback_submissions_kpi_submission_period_id ON metrics.case_feedback_submissions (kpi_submission_period_id);

CREATE UNIQUE INDEX ix_case_feedback_submissions_lookup_id ON metrics.case_feedback_submissions (lookup_id);

ALTER TABLE metrics.case_feedback_submissions ADD CONSTRAINT fk_case_feedback_submissions_kpi_submission_periods_kpi_submis FOREIGN KEY (kpi_submission_period_id) REFERENCES metrics.kpi_submission_periods (id) ON DELETE RESTRICT;

INSERT INTO metrics.__ef_migrations_history (migration_id, product_version)
VALUES ('20250718073414_006_AddColumnsToCaseFeedback', '9.0.2');

COMMIT;

