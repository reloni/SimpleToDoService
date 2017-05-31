#!/bin/bash

if [ -f "${PGDATA_BACKUP}" ]; then
  gunzip -c ${PGDATA_BACKUP} | psql --dbname=postgresql://${POSTGRES_USER}:${POSTGRES_PASSWORD}@${POSTGRES_HOST}:${POSTGRES_PORT}/${POSTGRES_DB} 2> ${BACKUP_RESTORE_LOG}
else
  cat /CreateDB.sql | psql --dbname=postgresql://${POSTGRES_USER}:${POSTGRES_PASSWORD}@${POSTGRES_HOST}:${POSTGRES_PORT}}/${POSTGRES_DB} 2> ${BACKUP_RESTORE_LOG}
fi
