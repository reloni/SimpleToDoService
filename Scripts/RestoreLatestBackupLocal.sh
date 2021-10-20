#!/bin/bash

set -e

export POSTGRES_USER=adminuser
# export POSTGRES_PASSWORD=password
export POSTGRES_HOST=localhost
export POSTGRES_PORT=5432
export POSTGRES_DB=todo-db-test
export SECRETS_BUCKET_REGION=us-east-1
export BACKUPS_BUCKET_NAME=task-manager-backups
export BACKUP_RESTORE_LOG=$PWD/restore.log

aws s3 --region ${SECRETS_BUCKET_REGION} cp s3://${BACKUPS_BUCKET_NAME}/latest.psql.gz ./latest.psql.gz
#restore backup
gunzip -c ./latest.psql.gz | psql --dbname=postgresql://${POSTGRES_USER}:${POSTGRES_PASSWORD}@${POSTGRES_HOST}:${POSTGRES_PORT}/${POSTGRES_DB} >> ${BACKUP_RESTORE_LOG} 2>&1 && \
rm ./latest.psql.gz
