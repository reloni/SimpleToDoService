#!/bin/bash

set -e -o pipefail

POSTGRES_TODO_PROD_USER=todouserprod
POSTGRES_TODO_DEV_USER=todouserdev
POSTGRES_ADMIN_USER=adminuser
POSTGRES_ADMIN_PASSWORD=$(aws secretsmanager get-secret-value --secret-id ArbitrraryBitsDatabaseAdminuserSecret --query SecretString --output text | jq -r ".password")
POSTGRES_TODO_PROD_PASSWORD=$(aws secretsmanager get-secret-value --secret-id ArbitrraryBitsDatabaseTodouserprodSecret --query SecretString --output text | jq -r ".password")
POSTGRES_TODO_DEV_PASSWORD=$(aws secretsmanager get-secret-value --secret-id ArbitrraryBitsDatabaseTodouserdevSecret --query SecretString --output text | jq -r ".password")
POSTGRES_HOST=localhost
POSTGRES_PORT=5432

cat ./update-user-password.sql \
  | sed 's/$USERNAME/'"$POSTGRES_TODO_PROD_USER"'/' \
  | sed 's/$PASSWORD/'"$POSTGRES_TODO_PROD_PASSWORD"'/' \
  > ./update-user-password-todouserprod.sql

cat ./update-user-password.sql \
  | sed 's/$USERNAME/'"$POSTGRES_TODO_DEV_USER"'/' \
  | sed 's/$PASSWORD/'"$POSTGRES_TODO_DEV_PASSWORD"'/' \
  > ./update-user-password-todouserdev.sql

psql --dbname=postgresql://${POSTGRES_ADMIN_USER}:${POSTGRES_ADMIN_PASSWORD}@localhost:5432/postgres \
    -f ./update-user-password-todouserprod.sql

psql --dbname=postgresql://${POSTGRES_ADMIN_USER}:${POSTGRES_ADMIN_PASSWORD}@localhost:5432/postgres \
    -f ./update-user-password-todouserdev.sql

rm ./update-user-password-todouserprod.sql
rm ./update-user-password-todouserdev.sql
