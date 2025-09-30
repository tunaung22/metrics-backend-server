START TRANSACTION;
DROP INDEX metrics.ix_case_feedback_score_submissions_case_feedback_id;

CREATE UNIQUE INDEX ix_case_feedback_score_submissions_feedback_id_user_id ON metrics.case_feedback_score_submissions (case_feedback_id, submitter_id);

INSERT INTO metrics.__ef_migrations_history (migration_id, product_version)
VALUES ('20250903045746_009_AddUniqueIndex_FeedbackScoreSubmission', '9.0.2');

COMMIT;

