#!/bin/bash

set -e

docker stop migrateempty
docker run -d --rm --name migrateempty -p 5432:5432 reloni/todo-postgres:latest

export POSTGRES_USER=postgres
export POSTGRES_PASSWORD=postgres
export POSTGRES_HOST=localhost
export POSTGRES_PORT=5432
export POSTGRES_DB=postgres
export MIGRATE_LOG=$PWD/migrate.log

sleep 7

sh MigrateDB.sh
