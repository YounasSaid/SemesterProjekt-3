CREATE TABLE users (
    user_id int GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    email varchar(255) NOT NULL UNIQUE,
    password_hash varchar(50) NOT NULL
);

INSERT INTO users (email, password_hash)
VALUES ('something@gmail.com', 232323232);

SELECT * FROM users;