-- Keycloak database initialization
CREATE DATABASE keycloak;
GRANT ALL PRIVILEGES ON DATABASE keycloak TO interstellar_user;

-- Application database initialization
-- (will use default 'interstellar' database)
CREATE SCHEMA IF NOT EXISTS app;
CREATE SCHEMA IF NOT EXISTS auth;

-- Grant permissions
GRANT ALL PRIVILEGES ON SCHEMA app TO interstellar_user;
GRANT ALL PRIVILEGES ON SCHEMA auth TO interstellar_user;
