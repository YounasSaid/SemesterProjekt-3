BEGIN;

CREATE EXTENSION IF NOT EXISTS citext;
CREATE EXTENSION IF NOT EXISTS pgcrypto;

CREATE TABLE IF NOT EXISTS users (
                                     id             uuid   PRIMARY KEY DEFAULT gen_random_uuid(),
                                     email          citext NOT NULL,
                                     password_hash  text   NOT NULL
);

DO $$
    BEGIN
        IF NOT EXISTS (
            SELECT 1 FROM pg_constraint WHERE conname = 'users_email_uk'
        ) THEN
            ALTER TABLE users ADD CONSTRAINT users_email_uk UNIQUE (email);
        END IF;
    END$$;

COMMIT;
