﻿START TRANSACTION;
DROP INDEX metrics.ix_case_feedback_score_submissions_feedback_id_user_id;

CREATE INDEX ix_case_feedback_score_submissions_case_feedback_id ON metrics.case_feedback_score_submissions (case_feedback_id);

DELETE FROM metrics.__ef_migrations_history
WHERE migration_id = '20250903045746_009_AddUniqueIndex_FeedbackScoreSubmission';

COMMIT;

