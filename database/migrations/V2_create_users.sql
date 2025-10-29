-- database/migrations/V3__add_user_fields.sql

-- Drop old table hvis den eksisterer
DROP TABLE IF EXISTS users CASCADE;

-- Opret ny korrekt tabel
CREATE TABLE users (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    email VARCHAR(255) NOT NULL,
    first_name VARCHAR(100) NOT NULL,
    last_name VARCHAR(100) NOT NULL,
    password_hash TEXT NOT NULL,
    semester SMALLINT NOT NULL,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT users_email_uk UNIQUE (LOWER(email))
);

CREATE INDEX idx_users_email_lower ON users (LOWER(email));
