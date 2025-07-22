START TRANSACTION;
DROP TABLE metrics.case_feedback_submissions;

DROP SEQUENCE metrics.case_feedback_submissions_id_seq;

DELETE FROM metrics.__ef_migrations_history
WHERE migration_id = '20250715040440_004_AddCaseFeedback';

COMMIT;

