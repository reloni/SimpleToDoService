#!/bin/bash

if [ "$LOAD_S3_SECRETS" = "YES" ]; then
  # Load the S3 secrets file contents into the environment variables
  eval $(aws s3 --region ${SECRETS_BUCKET_REGION} cp s3://${SECRETS_BUCKET_NAME}/${SECRETS_FILE_NAME} - | sed 's/^/export /')
fi

if [ -f "${PGDATA_BACKUP}" ]; then
  gunzip -c ${PGDATA_BACKUP} | psql --dbname=postgresql://${POSTGRES_USER}:${POSTGRES_PASSWORD}@${POSTGRES_HOST}:${POSTGRES_PORT}/${POSTGRES_DB} 2> ${BACKUP_RESTORE_LOG} && \
  rm ${PGDATA_BACKUP}
else
  cat /CreateDB.sql | psql --dbname=postgresql://${POSTGRES_USER}:${POSTGRES_PASSWORD}@${POSTGRES_HOST}:${POSTGRES_PORT}/${POSTGRES_DB} 2> ${BACKUP_RESTORE_LOG}
fi
