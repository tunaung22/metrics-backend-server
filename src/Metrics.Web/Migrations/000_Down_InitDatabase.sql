START TRANSACTION;
DROP TABLE metrics.asp_net_role_claims;

DROP TABLE metrics.asp_net_user_claims;

DROP TABLE metrics.asp_net_user_logins;

DROP TABLE metrics.asp_net_user_roles;

DROP TABLE metrics.asp_net_user_tokens;

DROP TABLE metrics.department_key_kpis;

DROP TABLE metrics.key_kpi_submission_items;

DROP TABLE metrics.kpi_submissions;

DROP TABLE metrics.application_roles;

DROP TABLE metrics.key_kpi_submissions;

DROP TABLE metrics.key_kpis;

DROP TABLE metrics.kpi_submission_periods;

DROP TABLE metrics.application_users;

DROP TABLE metrics.departments;

DROP TABLE metrics.user_titles;

DROP SEQUENCE metrics.department_key_kpis_id_seq;

DROP SEQUENCE metrics.departments_id_seq;

DROP SEQUENCE metrics.key_kpi_submission_items_id_seq;

DROP SEQUENCE metrics.key_kpi_submissions_id_seq;

DROP SEQUENCE metrics.key_kpis_id_seq;

DROP SEQUENCE metrics.kpi_submission_periods_id_seq;

DROP SEQUENCE metrics.kpi_submissions_id_seq;

DROP SEQUENCE metrics.user_titles_id_seq;

DELETE FROM metrics.__ef_migrations_history
WHERE migration_id = '20250523094849_000_InitDatabase';

COMMIT;

