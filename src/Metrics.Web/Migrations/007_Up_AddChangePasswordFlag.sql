START TRANSACTION;
ALTER TABLE metrics.application_users ADD is_password_change_required boolean NOT NULL DEFAULT TRUE;

INSERT INTO metrics.__ef_migrations_history (migration_id, product_version)
VALUES ('20250729170257_007_AddChangePasswordFlag', '9.0.2');

COMMIT;

