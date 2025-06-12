START TRANSACTION;
ALTER TABLE metrics.key_kpi_submission_items DROP CONSTRAINT fk_key_kpi_submission_items_departments_department_id;

ALTER TABLE metrics.key_kpi_submission_items DROP CONSTRAINT fk_key_kpi_submission_items_key_kpis_key_kpi_metrics_id;

ALTER TABLE metrics.key_kpi_submissions DROP CONSTRAINT fk_key_kpi_submissions_departments_department_id;

ALTER TABLE metrics.key_kpi_submissions DROP CONSTRAINT fk_key_kpi_submissions_key_kpis_key_kpi_id;

DROP TABLE metrics.department_key_kpis;

DROP TABLE metrics.key_kpis;

DROP INDEX metrics.ix_key_kpi_submissions_score_submission_period_id_application_;

DROP INDEX metrics.ix_key_kpi_submission_items_department_id;

DROP INDEX metrics.ix_key_kpi_submission_items_key_kpi_submission_id_key_kpi_metr;

ALTER TABLE metrics.key_kpi_submission_items DROP COLUMN department_id;

DROP SEQUENCE metrics.department_key_kpis_id_seq;

DROP SEQUENCE metrics.key_kpis_id_seq;

ALTER TABLE metrics.key_kpi_submissions RENAME COLUMN key_kpi_id TO key_metric_id;

ALTER INDEX metrics.ix_key_kpi_submissions_key_kpi_id RENAME TO ix_key_kpi_submissions_key_metric_id;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM pg_namespace WHERE nspname = 'metrics') THEN
        CREATE SCHEMA metrics;
    END IF;
END $EF$;

CREATE EXTENSION IF NOT EXISTS citext;

CREATE SEQUENCE metrics.department_key_metrics_id_seq START WITH 1 INCREMENT BY 10 NO CYCLE;

CREATE SEQUENCE metrics.key_metrics_id_seq START WITH 1 INCREMENT BY 10 NO CYCLE;

ALTER TABLE metrics.user_titles ALTER COLUMN title_name TYPE citext;

UPDATE metrics.key_kpi_submissions SET department_id = 0 WHERE department_id IS NULL;
ALTER TABLE metrics.key_kpi_submissions ALTER COLUMN department_id SET NOT NULL;
ALTER TABLE metrics.key_kpi_submissions ALTER COLUMN department_id SET DEFAULT 0;

ALTER TABLE metrics.key_kpi_submissions ADD department_key_metric_id bigint;

ALTER TABLE metrics.key_kpi_submission_items ADD key_metric_id bigint;

ALTER TABLE metrics.departments ALTER COLUMN department_name TYPE citext;

CREATE TABLE metrics.key_metrics (
    id bigint NOT NULL,
    metric_code uuid NOT NULL,
    metric_title text NOT NULL,
    description text,
    is_deleted boolean NOT NULL DEFAULT FALSE,
    created_at timestamp with time zone NOT NULL,
    modified_at timestamp with time zone NOT NULL,
    CONSTRAINT pk_key_metrics PRIMARY KEY (id)
);

CREATE TABLE metrics.department_key_metrics (
    id bigint NOT NULL,
    department_key_metric_code uuid NOT NULL,
    is_deleted boolean NOT NULL DEFAULT FALSE,
    created_at timestamp with time zone NOT NULL,
    modified_at timestamp with time zone NOT NULL,
    kpi_submission_period_id bigint NOT NULL,
    department_id bigint NOT NULL,
    key_metric_id bigint NOT NULL,
    CONSTRAINT pk_department_key_metrics PRIMARY KEY (id),
    CONSTRAINT fk_department_key_metrics_departments_department_id FOREIGN KEY (department_id) REFERENCES metrics.departments (id) ON DELETE RESTRICT,
    CONSTRAINT fk_department_key_metrics_key_metrics_key_metric_id FOREIGN KEY (key_metric_id) REFERENCES metrics.key_metrics (id) ON DELETE RESTRICT,
    CONSTRAINT fk_department_key_metrics_kpi_submission_periods_kpi_submissio FOREIGN KEY (kpi_submission_period_id) REFERENCES metrics.kpi_submission_periods (id) ON DELETE RESTRICT
);

CREATE INDEX ix_key_kpi_submissions_department_key_metric_id ON metrics.key_kpi_submissions (department_key_metric_id);

CREATE UNIQUE INDEX ix_key_kpi_submissions_score_submission_period_id_department_i ON metrics.key_kpi_submissions (score_submission_period_id, department_id, application_user_id);

CREATE UNIQUE INDEX ix_key_kpi_submission_items_key_kpi_submission_id_key_kpi_metr ON metrics.key_kpi_submission_items (key_kpi_submission_id, key_kpi_metrics_id);

CREATE INDEX ix_key_kpi_submission_items_key_metric_id ON metrics.key_kpi_submission_items (key_metric_id);

CREATE INDEX ix_department_key_metrics_department_id ON metrics.department_key_metrics (department_id);

CREATE INDEX ix_department_key_metrics_key_metric_id ON metrics.department_key_metrics (key_metric_id);

CREATE UNIQUE INDEX ix_department_key_metrics_kpi_submission_period_id_department_ ON metrics.department_key_metrics (kpi_submission_period_id, department_id, key_metric_id);

CREATE UNIQUE INDEX ix_key_metrics_metric_code ON metrics.key_metrics (metric_code);

CREATE UNIQUE INDEX ix_key_metrics_metric_title ON metrics.key_metrics (metric_title);

ALTER TABLE metrics.key_kpi_submission_items ADD CONSTRAINT fk_key_kpi_submission_items_department_key_metrics_key_kpi_met FOREIGN KEY (key_kpi_metrics_id) REFERENCES metrics.department_key_metrics (id) ON DELETE RESTRICT;

ALTER TABLE metrics.key_kpi_submission_items ADD CONSTRAINT fk_key_kpi_submission_items_key_metrics_key_metric_id FOREIGN KEY (key_metric_id) REFERENCES metrics.key_metrics (id);

ALTER TABLE metrics.key_kpi_submissions ADD CONSTRAINT fk_key_kpi_submissions_department_key_metrics_department_key_m FOREIGN KEY (department_key_metric_id) REFERENCES metrics.department_key_metrics (id);

ALTER TABLE metrics.key_kpi_submissions ADD CONSTRAINT fk_key_kpi_submissions_departments_department_id FOREIGN KEY (department_id) REFERENCES metrics.departments (id) ON DELETE RESTRICT;

ALTER TABLE metrics.key_kpi_submissions ADD CONSTRAINT fk_key_kpi_submissions_key_metrics_key_metric_id FOREIGN KEY (key_metric_id) REFERENCES metrics.key_metrics (id);

INSERT INTO metrics.__ef_migrations_history (migration_id, product_version)
VALUES ('20250612005610_001_AddKeyMetrics', '9.0.2');

COMMIT;

