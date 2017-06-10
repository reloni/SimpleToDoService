#!/bin/bash

DBVERSION=$(psql --dbname=postgresql://${POSTGRES_USER}:${POSTGRES_PASSWORD}@${POSTGRES_HOST}:${POSTGRES_PORT}/${POSTGRES_DB} -t -c 'select version from dbversion limit 1;')

if [ "$DBVERSION" -eq "1" ]; then
	cat ./Migrations/1.sql | psql --dbname=postgresql://${POSTGRES_USER}:${POSTGRES_PASSWORD}@${POSTGRES_HOST}:${POSTGRES_PORT}/${POSTGRES_DB} > ${MIGRATE_LOG} 2>&1
elif [ "$DBVERSION" -eq "2" ]; then
	echo "two"
else
	echo "other"
fi
