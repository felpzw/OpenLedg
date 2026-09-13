#!/bin/sh
set -eu
cd "$(dirname "$0")/.."
docker compose --profile auxiliary config --quiet
docker compose --profile auxiliary up -d --build --wait --wait-timeout 180
