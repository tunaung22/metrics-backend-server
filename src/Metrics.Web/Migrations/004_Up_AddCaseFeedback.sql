START TRANSACTION;
CREATE SEQUENCE metrics.case_feedback_submissions_id_seq START WITH 1 INCREMENT BY 10 NO CYCLE;

CREATE TABLE metrics.case_feedback_submissions (
    id bigint NOT NULL,
    submitted_at timestamp with time zone NOT NULL,
    submission_date date GENERATED ALWAYS AS ((submitted_at AT TIME ZONE 'UTC')::date) STORED NOT NULL,
    negative_score_value numeric(4,2) NOT NULL,
    submitter_id text NOT NULL,
    case_department_id bigint NOT NULL,
    ward_name citext NOT NULL,
    cpi_number citext NOT NULL,
    patient_name citext NOT NULL,
    room_number citext NOT NULL,
    incident_at timestamp with time zone NOT NULL,
    description text,
    comments text,
    is_deleted boolean NOT NULL DEFAULT FALSE,
    created_at timestamp with time zone NOT NULL,
    modified_at timestamp with time zone NOT NULL,
    CONSTRAINT pk_case_feedback_submissions PRIMARY KEY (id),
    CONSTRAINT fk_case_feedback_submissions_application_users_submitter_id FOREIGN KEY (submitter_id) REFERENCES metrics.application_users (id) ON DELETE RESTRICT,
    CONSTRAINT fk_case_feedback_submissions_departments_case_department_id FOREIGN KEY (case_department_id) REFERENCES metrics.departments (id) ON DELETE RESTRICT
);

CREATE INDEX ix_case_feedback_submissions_case_department_id ON metrics.case_feedback_submissions (case_department_id);

CREATE INDEX ix_case_feedback_submissions_submitter_id ON metrics.case_feedback_submissions (submitter_id);

INSERT INTO metrics.__ef_migrations_history (migration_id, product_version)
VALUES ('20250715040440_004_AddCaseFeedback', '9.0.2');

COMMIT;

