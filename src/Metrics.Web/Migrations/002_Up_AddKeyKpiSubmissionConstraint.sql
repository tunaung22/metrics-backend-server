START TRANSACTION;
CREATE SEQUENCE metrics.key_kpi_submission_constraints_id_seq START WITH 1 INCREMENT BY 10 NO CYCLE;

CREATE TABLE metrics.key_kpi_submission_constraints (
    id bigint NOT NULL,
    lookup_id uuid NOT NULL,
    is_deleted boolean NOT NULL DEFAULT FALSE,
    created_at timestamp with time zone NOT NULL,
    modified_at timestamp with time zone NOT NULL,
    department_id bigint NOT NULL,
    department_key_metric_id bigint NOT NULL,
    CONSTRAINT pk_key_kpi_submission_constraints PRIMARY KEY (id),
    CONSTRAINT fk_key_kpi_submission_constraints_department_key_metrics_depar FOREIGN KEY (department_key_metric_id) REFERENCES metrics.department_key_metrics (id) ON DELETE RESTRICT,
    CONSTRAINT fk_key_kpi_submission_constraints_departments_department_id FOREIGN KEY (department_id) REFERENCES metrics.departments (id) ON DELETE RESTRICT
);

CREATE UNIQUE INDEX ix_key_kpi_submission_constraints_department_id_department_key ON metrics.key_kpi_submission_constraints (department_id, department_key_metric_id);

CREATE INDEX ix_key_kpi_submission_constraints_department_key_metric_id ON metrics.key_kpi_submission_constraints (department_key_metric_id);

INSERT INTO metrics.__ef_migrations_history (migration_id, product_version)
VALUES ('20250701110033_002_AddKeyKpiSubmissionConstraint', '9.0.2');

COMMIT;

