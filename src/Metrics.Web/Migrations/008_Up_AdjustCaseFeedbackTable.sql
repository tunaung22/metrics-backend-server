START TRANSACTION;
DROP TABLE metrics.case_feedback_submissions;

DROP SEQUENCE metrics.case_feedback_submissions_id_seq;

CREATE SEQUENCE metrics.case_feedback_score_submissions_id_seq START WITH 1 INCREMENT BY 10 NO CYCLE;

CREATE SEQUENCE metrics.case_feedbacks_id_seq START WITH 1 INCREMENT BY 10 NO CYCLE;

CREATE TABLE metrics.case_feedbacks (
    id bigint NOT NULL,
    lookup_id uuid NOT NULL,
    kpi_submission_period_id bigint NOT NULL,
    submitted_at timestamp with time zone NOT NULL,
    submission_date date GENERATED ALWAYS AS ((submitted_at AT TIME ZONE 'UTC')::date) STORED NOT NULL,
    submitter_id text NOT NULL,
    case_department_id bigint NOT NULL,
    ward_name citext NOT NULL,
    cpi_number citext NOT NULL,
    patient_name citext NOT NULL,
    room_number citext NOT NULL,
    incident_at timestamp with time zone NOT NULL,
    description text,
    is_deleted boolean NOT NULL DEFAULT FALSE,
    created_at timestamp with time zone NOT NULL,
    modified_at timestamp with time zone NOT NULL,
    CONSTRAINT pk_case_feedbacks PRIMARY KEY (id),
    CONSTRAINT fk_case_feedbacks_application_users_submitter_id FOREIGN KEY (submitter_id) REFERENCES metrics.application_users (id) ON DELETE RESTRICT,
    CONSTRAINT fk_case_feedbacks_departments_case_department_id FOREIGN KEY (case_department_id) REFERENCES metrics.departments (id) ON DELETE RESTRICT,
    CONSTRAINT fk_case_feedbacks_kpi_submission_periods_kpi_submission_period FOREIGN KEY (kpi_submission_period_id) REFERENCES metrics.kpi_submission_periods (id) ON DELETE RESTRICT
);

CREATE TABLE metrics.case_feedback_score_submissions (
    id bigint NOT NULL,
    lookup_id uuid NOT NULL,
    submitted_at timestamp with time zone NOT NULL,
    submission_date date GENERATED ALWAYS AS ((submitted_at AT TIME ZONE 'UTC')::date) STORED NOT NULL,
    negative_score_value numeric(4,2) NOT NULL,
    comments text,
    submitter_id text NOT NULL,
    case_feedback_id bigint NOT NULL,
    is_deleted boolean NOT NULL DEFAULT FALSE,
    created_at timestamp with time zone NOT NULL,
    modified_at timestamp with time zone NOT NULL,
    CONSTRAINT pk_case_feedback_score_submissions PRIMARY KEY (id),
    CONSTRAINT fk_case_feedback_score_submissions_application_users_submitter FOREIGN KEY (submitter_id) REFERENCES metrics.application_users (id) ON DELETE RESTRICT,
    CONSTRAINT fk_casefeedbacksubmissions_casefeedbacks_case_feedback_id FOREIGN KEY (case_feedback_id) REFERENCES metrics.case_feedbacks (id) ON DELETE RESTRICT
);

CREATE INDEX ix_case_feedback_score_submissions_case_feedback_id ON metrics.case_feedback_score_submissions (case_feedback_id);

CREATE UNIQUE INDEX ix_case_feedback_score_submissions_lookup_id ON metrics.case_feedback_score_submissions (lookup_id);

CREATE INDEX ix_case_feedback_score_submissions_submitter_id ON metrics.case_feedback_score_submissions (submitter_id);

CREATE INDEX ix_case_feedbacks_case_department_id ON metrics.case_feedbacks (case_department_id);

CREATE INDEX ix_case_feedbacks_kpi_submission_period_id ON metrics.case_feedbacks (kpi_submission_period_id);

CREATE UNIQUE INDEX ix_case_feedbacks_lookup_id ON metrics.case_feedbacks (lookup_id);

CREATE INDEX ix_case_feedbacks_submitter_id ON metrics.case_feedbacks (submitter_id);

INSERT INTO metrics.__ef_migrations_history (migration_id, product_version)
VALUES ('20250821043714_008_AdjustCaseFeedbackTable', '9.0.2');

COMMIT;

