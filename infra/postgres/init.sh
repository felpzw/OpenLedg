#!/bin/sh
set -eu

psql --username "$POSTGRES_USER" --dbname "$POSTGRES_DB" --set=ON_ERROR_STOP=1 <<'SQL'
\getenv app_password POSTGRES_APP_PASSWORD
CREATE ROLE openledg_app LOGIN PASSWORD :'app_password';
REVOKE ALL ON DATABASE openledg FROM PUBLIC;
GRANT CONNECT ON DATABASE openledg TO openledg_app;
REVOKE CREATE ON SCHEMA public FROM PUBLIC;
CREATE SCHEMA olgp AUTHORIZATION openledg_owner;
CREATE SCHEMA security AUTHORIZATION openledg_owner;
CREATE SCHEMA routing AUTHORIZATION openledg_owner;
REVOKE ALL ON SCHEMA security FROM PUBLIC;
GRANT USAGE ON SCHEMA olgp, routing TO openledg_app;
ALTER DEFAULT PRIVILEGES FOR ROLE openledg_owner IN SCHEMA olgp, routing
    GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO openledg_app;
SQL
