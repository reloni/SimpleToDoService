#!/bin/bash

set -e

DBTYPE=$1

if [ "$DBTYPE" = "prod" ]; then
  DB=todoprod
  POSTGRES_TODO_USER=todouserprod
  POSTGRES_TODO_PASSWORD=$(aws secretsmanager get-secret-value --secret-id ArbitrraryBitsDatabaseTodouserprodSecret --query SecretString --output text | jq -r ".password")
elif [ "$DBTYPE" = "dev" ]; then
  DB=tododev
  POSTGRES_TODO_USER=todouserdev
  POSTGRES_TODO_PASSWORD=$(aws secretsmanager get-secret-value --secret-id ArbitrraryBitsDatabaseTodouserdevSecret --query SecretString --output text | jq -r ".password")
else
  echo "Unknown dbtype"
  exit 1
fi

echo "Restore DB $DBTYPE"

SCHEMA_NAME=todoservice
POSTGRES_ADMIN_USER=adminuser
POSTGRES_ADMIN_PASSWORD=$(aws secretsmanager get-secret-value --secret-id ArbitrraryBitsDatabaseAdminuserSecret --query SecretString --output text | jq -r ".password")
POSTGRES_HOST=localhost
POSTGRES_PORT=5432
SECRETS_BUCKET_REGION=us-east-1
BACKUPS_BUCKET_NAME=task-manager-backups
BACKUP_RESTORE_LOG=$PWD/restore.log

cat ./initialize-db-schema.sql \
  | sed 's/$DB/'"$DB"'/' \
  | sed 's/$SCHEMA_NAME/'"$SCHEMA_NAME"'/' \
  | sed 's/$USERNAME/'"$POSTGRES_TODO_USER"'/' \
  | sed 's/$PASSWORD/'"$POSTGRES_TODO_PASSWORD"'/' \
  > ./initialize-schema-initialized.sql

psql --dbname=postgresql://${POSTGRES_ADMIN_USER}:${POSTGRES_ADMIN_PASSWORD}@localhost:5432/postgres \
    -f ./initialize-schema-initialized.sql

rm ./initialize-schema-initialized.sql

aws s3 --region ${SECRETS_BUCKET_REGION} cp s3://${BACKUPS_BUCKET_NAME}/latest.psql.gz ./latest.psql.gz
# #restore backup
gunzip -c ./latest.psql.gz | psql --dbname=postgresql://${POSTGRES_ADMIN_USER}:${POSTGRES_ADMIN_PASSWORD}@${POSTGRES_HOST}:${POSTGRES_PORT}/${DB} >> ${BACKUP_RESTORE_LOG} 2>&1 && \
  rm ./latest.psql.gz

cat ./rename-schema.sql \
  | sed 's/$DB/'"$DB"'/' \
  | sed 's/$SCHEMA_NAME/'"$SCHEMA_NAME"'/' \
  | sed 's/$USERNAME/'"$POSTGRES_TODO_USER"'/' \
  > ./rename-schema-initialized.sql

psql --dbname=postgresql://${POSTGRES_ADMIN_USER}:${POSTGRES_ADMIN_PASSWORD}@localhost:5432/postgres \
  -f ./rename-schema-initialized.sql

rm ./rename-schema-initialized.sql
