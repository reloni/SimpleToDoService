#!/bin/bash

if [ "$LOAD_S3_SECRETS" = "YES" ]; then
  # Load the S3 secrets file contents into the environment variables
  eval $(aws s3 --region ${SECRETS_BUCKET_REGION} cp s3://${SECRETS_BUCKET_NAME}/${SECRETS_FILE_NAME} - | sed 's/^/export /')
fi

#drop database
psql --dbname=postgresql://${POSTGRES_USER}:${POSTGRES_PASSWORD}@${POSTGRES_HOST}:${POSTGRES_PORT}/${POSTGRES_DB} -t -c "drop database if exists ${POSTGRES_DB};" >> ${BACKUP_RESTORE_LOG} 2>&1

#load latest database backup
aws s3 --region ${SECRETS_BUCKET_REGION} cp s3://${BACKUPS_BUCKET_NAME}/latest.psql.gz ./latest.psql.gz > ${BACKUP_RESTORE_LOG} 2>&1
#restore backup
gunzip -c ./latest.psql.gz | psql --dbname=postgresql://${POSTGRES_USER}:${POSTGRES_PASSWORD}@${POSTGRES_HOST}:${POSTGRES_PORT}/${POSTGRES_DB} >> ${BACKUP_RESTORE_LOG} 2>&1 && \
rm ./latest.psql.gz
