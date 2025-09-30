START TRANSACTION;
ALTER TABLE metrics.key_kpi_submissions DROP CONSTRAINT fk_key_kpi_submissions_dkm_id;

ALTER TABLE metrics.key_kpi_submissions DROP CONSTRAINT fk_key_kpi_submissions_submitter_id;

DROP INDEX metrics.ix_key_kpi_submissions_dkm_id_user_id;

ALTER TABLE metrics.key_kpi_submissions DROP COLUMN comments;

ALTER TABLE metrics.key_kpi_submissions DROP COLUMN score_value;

ALTER TABLE metrics.key_kpi_submissions RENAME COLUMN submitter_id TO application_user_id;

ALTER TABLE metrics.key_kpi_submissions RENAME COLUMN department_key_metric_id TO score_submission_period_id;

ALTER INDEX metrics.ix_key_kpi_submissions_submitter_id RENAME TO ix_key_kpi_submissions_application_user_id;

CREATE SEQUENCE metrics.key_kpi_submission_items_id_seq START WITH 1 INCREMENT BY 10 NO CYCLE;

ALTER TABLE metrics.key_kpi_submissions ADD department_id bigint NOT NULL DEFAULT 0;

CREATE TABLE metrics.key_kpi_submission_items (
    id bigint NOT NULL,
    department_key_metric_id bigint NOT NULL,
    key_kpi_submission_id bigint NOT NULL,
    comments text,
    created_at timestamp with time zone NOT NULL,
    is_deleted boolean NOT NULL DEFAULT FALSE,
    modified_at timestamp with time zone NOT NULL,
    score_value numeric(4,2) NOT NULL,
    CONSTRAINT pk_key_kpi_submission_items PRIMARY KEY (id),
    CONSTRAINT ck_kpi_submissions_kpi_score_gt_0 CHECK (score_value >= 0),
    CONSTRAINT fk_key_kpi_submission_items_department_key_metrics_department_ FOREIGN KEY (department_key_metric_id) REFERENCES metrics.department_key_metrics (id) ON DELETE RESTRICT,
    CONSTRAINT fk_key_kpi_submission_items_key_kpi_submissions_key_kpi_submis FOREIGN KEY (key_kpi_submission_id) REFERENCES metrics.key_kpi_submissions (id) ON DELETE RESTRICT
);

CREATE INDEX ix_key_kpi_submissions_department_id ON metrics.key_kpi_submissions (department_id);

CREATE UNIQUE INDEX ix_key_kpi_submissions_period_id_dpt_id_user_id ON metrics.key_kpi_submissions (score_submission_period_id, department_id, application_user_id);

CREATE INDEX ix_key_kpi_submission_items_department_key_metric_id ON metrics.key_kpi_submission_items (department_key_metric_id);

CREATE UNIQUE INDEX ix_key_kpi_submission_items_kks_id_dkm_id ON metrics.key_kpi_submission_items (key_kpi_submission_id, department_key_metric_id);

ALTER TABLE metrics.key_kpi_submissions ADD CONSTRAINT fk_key_kpi_submissions_departments_department_id FOREIGN KEY (department_id) REFERENCES metrics.departments (id) ON DELETE RESTRICT;

ALTER TABLE metrics.key_kpi_submissions ADD CONSTRAINT fk_key_kpi_submissions_kpi_submission_periods_score_submission FOREIGN KEY (score_submission_period_id) REFERENCES metrics.kpi_submission_periods (id) ON DELETE RESTRICT;

ALTER TABLE metrics.key_kpi_submissions ADD CONSTRAINT fk_key_kpi_submissions_users_application_user_id FOREIGN KEY (application_user_id) REFERENCES metrics.application_users (id) ON DELETE RESTRICT;

DELETE FROM metrics.__ef_migrations_history
WHERE migration_id = '20250923045713_013_AlterKeyKpiSubmissionTable';

COMMIT;

