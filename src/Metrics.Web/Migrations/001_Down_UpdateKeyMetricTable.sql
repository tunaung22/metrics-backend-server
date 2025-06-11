START TRANSACTION;
ALTER TABLE metrics.key_kpi_submission_items DROP CONSTRAINT fk_key_kpi_submission_items_department_key_metrics_key_kpi_met;

ALTER TABLE metrics.key_kpi_submission_items DROP CONSTRAINT fk_key_kpi_submission_items_departments_department_id;

ALTER TABLE metrics.key_kpi_submission_items DROP CONSTRAINT fk_key_kpi_submission_items_key_metrics_key_metric_id;

ALTER TABLE metrics.key_kpi_submissions DROP CONSTRAINT fk_key_kpi_submissions_department_key_metrics_department_key_m;

ALTER TABLE metrics.key_kpi_submissions DROP CONSTRAINT fk_key_kpi_submissions_departments_department_id;

ALTER TABLE metrics.key_kpi_submissions DROP CONSTRAINT fk_key_kpi_submissions_departments_department_id1;

ALTER TABLE metrics.key_kpi_submissions DROP CONSTRAINT fk_key_kpi_submissions_key_metrics_key_metric_id;

DROP TABLE metrics.department_key_metrics;

DROP TABLE metrics.key_metrics;

DROP INDEX metrics.ix_key_kpi_submissions_department_id1;

DROP INDEX metrics.ix_key_kpi_submissions_department_key_metric_id;

DROP INDEX metrics.ix_key_kpi_submissions_score_submission_period_id_department_i;

DROP INDEX metrics.ix_key_kpi_submission_items_key_kpi_submission_id_key_kpi_metr;

DROP INDEX metrics.ix_key_kpi_submission_items_key_metric_id;

ALTER TABLE metrics.key_kpi_submissions DROP COLUMN department_id1;

ALTER TABLE metrics.key_kpi_submissions DROP COLUMN department_key_metric_id;

ALTER TABLE metrics.key_kpi_submission_items DROP COLUMN key_metric_id;

DROP SEQUENCE metrics.department_key_metrics_id_seq;

DROP SEQUENCE metrics.key_metrics_id_seq;

ALTER TABLE metrics.key_kpi_submissions RENAME COLUMN key_metric_id TO key_kpi_id;

ALTER INDEX metrics.ix_key_kpi_submissions_key_metric_id RENAME TO ix_key_kpi_submissions_key_kpi_id;

CREATE SEQUENCE metrics.department_key_kpis_id_seq START WITH 1 INCREMENT BY 10 NO CYCLE;

CREATE SEQUENCE metrics.key_kpis_id_seq START WITH 1 INCREMENT BY 10 NO CYCLE;

ALTER TABLE metrics.key_kpi_submissions ALTER COLUMN department_id DROP NOT NULL;

UPDATE metrics.key_kpi_submission_items SET department_id = 0 WHERE department_id IS NULL;
ALTER TABLE metrics.key_kpi_submission_items ALTER COLUMN department_id SET NOT NULL;
ALTER TABLE metrics.key_kpi_submission_items ALTER COLUMN department_id SET DEFAULT 0;

CREATE TABLE metrics.key_kpis (
    id bigint NOT NULL,
    created_at timestamp with time zone NOT NULL,
    description text,
    is_deleted boolean NOT NULL DEFAULT FALSE,
    metric_code uuid NOT NULL,
    metric_title text NOT NULL,
    modified_at timestamp with time zone NOT NULL,
    CONSTRAINT pk_key_kpis PRIMARY KEY (id)
);

CREATE TABLE metrics.department_key_kpis (
    id bigint NOT NULL,
    department_id bigint NOT NULL,
    key_kpi_metric_id bigint NOT NULL,
    created_at timestamp with time zone NOT NULL,
    is_deleted boolean NOT NULL DEFAULT FALSE,
    modified_at timestamp with time zone NOT NULL,
    CONSTRAINT pk_department_key_kpis PRIMARY KEY (id),
    CONSTRAINT fk_department_key_kpis_departments_department_id FOREIGN KEY (department_id) REFERENCES metrics.departments (id) ON DELETE RESTRICT,
    CONSTRAINT fk_department_key_kpis_key_kpis_key_kpi_metric_id FOREIGN KEY (key_kpi_metric_id) REFERENCES metrics.key_kpis (id) ON DELETE RESTRICT
);

CREATE UNIQUE INDEX ix_key_kpi_submissions_score_submission_period_id_application_ ON metrics.key_kpi_submissions (score_submission_period_id, application_user_id);

CREATE UNIQUE INDEX ix_key_kpi_submission_items_key_kpi_submission_id_key_kpi_metr ON metrics.key_kpi_submission_items (key_kpi_submission_id, key_kpi_metrics_id, department_id);

CREATE INDEX ix_department_key_kpis_department_id ON metrics.department_key_kpis (department_id);

CREATE INDEX ix_department_key_kpis_key_kpi_metric_id ON metrics.department_key_kpis (key_kpi_metric_id);

CREATE UNIQUE INDEX ix_key_kpis_metric_code ON metrics.key_kpis (metric_code);

CREATE UNIQUE INDEX ix_key_kpis_metric_title ON metrics.key_kpis (metric_title);

ALTER TABLE metrics.key_kpi_submission_items ADD CONSTRAINT fk_key_kpi_submission_items_departments_department_id FOREIGN KEY (department_id) REFERENCES metrics.departments (id) ON DELETE RESTRICT;

ALTER TABLE metrics.key_kpi_submission_items ADD CONSTRAINT fk_key_kpi_submission_items_key_kpis_key_kpi_metrics_id FOREIGN KEY (key_kpi_metrics_id) REFERENCES metrics.key_kpis (id) ON DELETE RESTRICT;

ALTER TABLE metrics.key_kpi_submissions ADD CONSTRAINT fk_key_kpi_submissions_departments_department_id FOREIGN KEY (department_id) REFERENCES metrics.departments (id);

ALTER TABLE metrics.key_kpi_submissions ADD CONSTRAINT fk_key_kpi_submissions_key_kpis_key_kpi_id FOREIGN KEY (key_kpi_id) REFERENCES metrics.key_kpis (id);

DELETE FROM metrics.__ef_migrations_history
WHERE migration_id = '20250605090306_001_UpdateKeyMetricTable';

COMMIT;

