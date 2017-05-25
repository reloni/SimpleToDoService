#!/bin/bash

echo "in custom script"
if [ -f "${PGDATA_BACKUP}" ]; then
  echo "try to restore db"
  gunzip -c ${PGDATA_BACKUP} | psql --dbname=postgresql://${POSTGRES_USER}:${POSTGRES_PASSWORD}}@${POSTGRES_HOST}:${POSTGRES_PORT}}/${POSTGRES_DB} 2> ${BACKUP_RESTORE_LOG}
  #rm -f ${PGDATA_BACKUP}
#else
  #cat 1_CreateTables.sql | psql --dbname=postgresql://postgres:psw@127.0.0.1:5432/postgres
fi
