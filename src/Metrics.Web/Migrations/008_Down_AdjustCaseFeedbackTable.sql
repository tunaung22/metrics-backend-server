START TRANSACTION;
DROP TABLE metrics.case_feedback_score_submissions;

DROP TABLE metrics.case_feedbacks;

DROP SEQUENCE metrics.case_feedback_score_submissions_id_seq;

DROP SEQUENCE metrics.case_feedbacks_id_seq;

CREATE SEQUENCE metrics.case_feedback_submissions_id_seq START WITH 1 INCREMENT BY 10 NO CYCLE;

CREATE TABLE metrics.case_feedback_submissions (
    id bigint NOT NULL,
    case_department_id bigint NOT NULL,
    kpi_submission_period_id bigint NOT NULL,
    submitter_id text NOT NULL,
    cpi_number citext NOT NULL,
    comments text,
    created_at timestamp with time zone NOT NULL,
    description text,
    incident_at timestamp with time zone NOT NULL,
    is_deleted boolean NOT NULL DEFAULT FALSE,
    lookup_id uuid NOT NULL,
    modified_at timestamp with time zone NOT NULL,
    negative_score_value numeric(4,2) NOT NULL,
    patient_name citext NOT NULL,
    room_number citext NOT NULL,
    submission_date date GENERATED ALWAYS AS ((submitted_at AT TIME ZONE 'UTC')::date) STORED NOT NULL,
    submitted_at timestamp with time zone NOT NULL,
    ward_name citext NOT NULL,
    CONSTRAINT pk_case_feedback_submissions PRIMARY KEY (id),
    CONSTRAINT fk_case_feedback_submissions_application_users_submitter_id FOREIGN KEY (submitter_id) REFERENCES metrics.application_users (id) ON DELETE RESTRICT,
    CONSTRAINT fk_case_feedback_submissions_departments_case_department_id FOREIGN KEY (case_department_id) REFERENCES metrics.departments (id) ON DELETE RESTRICT,
    CONSTRAINT fk_case_feedback_submissions_kpi_submission_periods_kpi_submis FOREIGN KEY (kpi_submission_period_id) REFERENCES metrics.kpi_submission_periods (id) ON DELETE RESTRICT
);

CREATE INDEX ix_case_feedback_submissions_case_department_id ON metrics.case_feedback_submissions (case_department_id);

CREATE INDEX ix_case_feedback_submissions_kpi_submission_period_id ON metrics.case_feedback_submissions (kpi_submission_period_id);

CREATE UNIQUE INDEX ix_case_feedback_submissions_lookup_id ON metrics.case_feedback_submissions (lookup_id);

CREATE INDEX ix_case_feedback_submissions_submitter_id ON metrics.case_feedback_submissions (submitter_id);

DELETE FROM metrics.__ef_migrations_history
WHERE migration_id = '20250821043714_008_AdjustCaseFeedbackTable';

COMMIT;

