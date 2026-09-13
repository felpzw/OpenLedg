#!/bin/sh
set -eu
cd "$(dirname "$0")/.."
if [ -f .env ]; then
    set -a
    . ./.env
    set +a
fi
: "${POSTGRES_PASSWORD:?Set POSTGRES_PASSWORD in .env}"
: "${POSTGRES_APP_PASSWORD:?Set POSTGRES_APP_PASSWORD in .env}"
export OPENLEDG_TEST_OWNER="Host=localhost;Port=${POSTGRES_PORT:-5432};Database=openledg;Username=openledg_owner;Password=$POSTGRES_PASSWORD"
export OPENLEDG_TEST_APP="Host=localhost;Port=${POSTGRES_PORT:-5432};Database=openledg;Username=openledg_app;Password=$POSTGRES_APP_PASSWORD"
dotnet test tests/OpenLedg.IntegrationTests "$@"
