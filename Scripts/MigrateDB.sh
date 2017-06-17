#!/bin/bash

if [ "$LOAD_S3_SECRETS" = "YES" ]; then
  # Load the S3 secrets file contents into the environment variables
  eval $(aws s3 --region ${SECRETS_BUCKET_REGION} cp s3://${SECRETS_BUCKET_NAME}/${SECRETS_FILE_NAME} - | sed 's/^/export /')
fi

DBVERSION=$(psql --dbname=postgresql://${POSTGRES_USER}:${POSTGRES_PASSWORD}@${POSTGRES_HOST}:${POSTGRES_PORT}/${POSTGRES_DB} -t -c 'select version from dbversion limit 1;')

if [ "$DBVERSION" -eq "0" ]; then
	echo "Initialize DB" >> ${MIGRATE_LOG}
	cat /CreateDB.sql | psql --dbname=postgresql://${POSTGRES_USER}:${POSTGRES_PASSWORD}@${POSTGRES_HOST}:${POSTGRES_PORT}/${POSTGRES_DB} > ${MIGRATE_LOG} 2>&1
	cat /Migrations/1.sql | psql --dbname=postgresql://${POSTGRES_USER}:${POSTGRES_PASSWORD}@${POSTGRES_HOST}:${POSTGRES_PORT}/${POSTGRES_DB} > ${MIGRATE_LOG} 2>&1
elif [ "$DBVERSION" -eq "1" ]; then
	echo "Migrate DB to ver 2" >> ${MIGRATE_LOG}
	cat /Migrations/1.sql | psql --dbname=postgresql://${POSTGRES_USER}:${POSTGRES_PASSWORD}@${POSTGRES_HOST}:${POSTGRES_PORT}/${POSTGRES_DB} > ${MIGRATE_LOG} 2>&1
else
	echo "Unknown DB version $DBVERSION" >> ${MIGRATE_LOG}
fi
