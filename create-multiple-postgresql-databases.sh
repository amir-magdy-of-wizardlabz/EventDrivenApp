#!/bin/bash
set -euo pipefail

# Creates one database per name in POSTGRES_MULTIPLE_DATABASES (comma-separated).
# Runs only the first time the container starts with an empty data directory.
if [ -n "${POSTGRES_MULTIPLE_DATABASES:-}" ]; then
  for db in ${POSTGRES_MULTIPLE_DATABASES//,/ }; do
    echo "Creating database '$db'"
    psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname postgres -c "CREATE DATABASE \"$db\";"
  done
fi
