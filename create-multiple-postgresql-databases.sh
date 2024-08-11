#!/bin/bash
set -e
set -o errexit

# Create user and databases
function create_user_and_database() {
  local DB_DATABASE=$1
  echo "Creating user and database '$DB_DATABASE'"
  psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" <<-EOSQL
    CREATE USER $DB_DATABASE WITH PASSWORD '${POSTGRES_PASSWORD}';
    CREATE DATABASE $DB_DATABASE;
    GRANT ALL PRIVILEGES ON DATABASE $DB_DATABASE TO $DB_DATABASE;
  EOSQL
}

if [ -n "$POSTGRES_MULTIPLE_DATABASES" ]; then
  echo "Multiple database creation requested: $POSTGRES_MULTIPLE_DATABASES"
  for db in $(echo "$POSTGRES_MULTIPLE_DATABASES" | tr ',' ' '); do
    create_user_and_database "$db"
  done
  echo "Multiple databases created"
fi
