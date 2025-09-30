START TRANSACTION;
ALTER TABLE metrics.key_kpi_submissions DROP CONSTRAINT fk_key_kpi_submissions_departments_department_id;

ALTER TABLE metrics.key_kpi_submissions DROP CONSTRAINT fk_key_kpi_submissions_kpi_submission_periods_score_submission;

ALTER TABLE metrics.key_kpi_submissions DROP CONSTRAINT fk_key_kpi_submissions_users_application_user_id;

DROP TABLE metrics.key_kpi_submission_items;

DROP INDEX metrics.ix_key_kpi_submissions_department_id;

DROP INDEX metrics.ix_key_kpi_submissions_period_id_dpt_id_user_id;

ALTER TABLE metrics.key_kpi_submissions DROP COLUMN department_id;

DROP SEQUENCE metrics.key_kpi_submission_items_id_seq;

ALTER TABLE metrics.key_kpi_submissions RENAME COLUMN score_submission_period_id TO department_key_metric_id;

ALTER TABLE metrics.key_kpi_submissions RENAME COLUMN application_user_id TO submitter_id;

ALTER INDEX metrics.ix_key_kpi_submissions_application_user_id RENAME TO ix_key_kpi_submissions_submitter_id;

ALTER TABLE metrics.key_kpi_submissions ADD comments text;

ALTER TABLE metrics.key_kpi_submissions ADD score_value numeric(4,2) NOT NULL DEFAULT 0.0;

CREATE UNIQUE INDEX ix_key_kpi_submissions_dkm_id_user_id ON metrics.key_kpi_submissions (department_key_metric_id, submitter_id);

ALTER TABLE metrics.key_kpi_submissions ADD CONSTRAINT fk_key_kpi_submissions_dkm_id FOREIGN KEY (department_key_metric_id) REFERENCES metrics.department_key_metrics (id) ON DELETE RESTRICT;

ALTER TABLE metrics.key_kpi_submissions ADD CONSTRAINT fk_key_kpi_submissions_submitter_id FOREIGN KEY (submitter_id) REFERENCES metrics.application_users (id) ON DELETE RESTRICT;

INSERT INTO metrics.__ef_migrations_history (migration_id, product_version)
VALUES ('20250923045713_013_AlterKeyKpiSubmissionTable', '9.0.4');

COMMIT;

