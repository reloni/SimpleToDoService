#!/bin/bash

set -e

export POSTGRES_USER=test
export POSTGRES_PASSWORD=password
export POSTGRES_HOST=localhost
export POSTGRES_PORT=5432
export POSTGRES_DB=lastdb
export MIGRATE_LOG=$PWD/migrate.log
export SECRETS_BUCKET_REGION=us-east-1
export BACKUPS_BUCKET_NAME=task-manager-backups
export BACKUP_RESTORE_LOG=$PWD/restore.log

docker stop migratelast || true
docker run -d --rm --name migratelast -e "POSTGRES_USER=${POSTGRES_USER}" -e "POSTGRES_PASSWORD=${POSTGRES_PASSWORD}" -e "POSTGRES_HOST=${POSTGRES_HOST}" -e "POSTGRES_PORT=${POSTGRES_PORT}" -e "POSTGRES_DB=${POSTGRES_DB}" -p 5432:5432 reloni/todo-postgres:latest

sleep 7

aws s3 --region ${SECRETS_BUCKET_REGION} cp s3://${BACKUPS_BUCKET_NAME}/latest.psql.gz ./latest.psql.gz
#restore backup
gunzip -c ./latest.psql.gz | psql --dbname=postgresql://${POSTGRES_USER}:${POSTGRES_PASSWORD}@${POSTGRES_HOST}:${POSTGRES_PORT}/${POSTGRES_DB} >> ${BACKUP_RESTORE_LOG} 2>&1 && \
rm ./latest.psql.gz

sh MigrateDB.sh
