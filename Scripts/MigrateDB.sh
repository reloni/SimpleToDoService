#!/bin/bash

if [ "$LOAD_S3_SECRETS" = "YES" ]; then
  # Load the S3 secrets file contents into the environment variables
  eval $(aws s3 --region ${SECRETS_BUCKET_REGION} cp s3://${SECRETS_BUCKET_NAME}/${SECRETS_FILE_NAME} - | sed 's/^/export /')
fi

DBVERSION=$(psql --dbname=postgresql://${POSTGRES_USER}:${POSTGRES_PASSWORD}@${POSTGRES_HOST}:${POSTGRES_PORT}/${POSTGRES_DB} -t -c 'select version from dbversion limit 1;')

echo "DB version before migrations $DBVERSION" >> ${MIGRATE_LOG}
(cd ./Migrations && ls | sort -n | xargs -n 1 psql --dbname=postgresql://${POSTGRES_USER}:${POSTGRES_PASSWORD}@${POSTGRES_HOST}:${POSTGRES_PORT}/${POSTGRES_DB} -f >> ${MIGRATE_LOG} 2>&1)

DBVERSION=$(psql --dbname=postgresql://${POSTGRES_USER}:${POSTGRES_PASSWORD}@${POSTGRES_HOST}:${POSTGRES_PORT}/${POSTGRES_DB} -t -c 'select version from dbversion limit 1;')
echo "DB version after migrations $DBVERSION" >> ${MIGRATE_LOG}
