docker run -d --name task-manager-service -p 5000:5000 -e POSTGRES_PASSWORD=test -e POSTGRES_USER=taskmanager -e POSTGRES_DB=tasksdb -e POSTGRES_HOST=localhost -e POSTGRES_PORT=5432 reloni/todo-service:latest


docker run -d --name task-manager-service -p 5000:5000 -e POSTGRES_PASSWORD=test -e POSTGRES_USER=taskmanager -e POSTGRES_DB=tasksdb -e POSTGRES_HOST=localhost -e POSTGRES_PORT=5432 test-todo

docker build -f Dockerfile.debug -t serv-restore .

docker run --name serv -p 5000:5000 -v /Users/AntonEfimenko/pgdata_back:/pgdata_back -e POSTGRES_PASSWORD=psw -e PGDATA_BACKUP=/pgdata_back/latest.psql.gz -e BACKUP_RESTORE_LOG=/pgdata_back/restore.log -e POSTGRES_USER=postgres -e POSTGRES_HOST=localhost -e POSTGRES_PORT=5432 -e POSTGRES_DB=postgres -i serv-restore

docker run --name serv -p 5000:5000 -v /Users/AntonEfimenko/pgdata_back:/pgdata_back --link pgs2:db -e POSTGRES_PASSWORD=psw -e PGDATA_BACKUP=/pgdata_back/latest.psql.gz -e BACKUP_RESTORE_LOG=/pgdata_back/restore.log -e POSTGRES_USER=postgres -e POSTGRES_HOST=db -e POSTGRES_PORT=5432 -e POSTGRES_DB=postgres -i serv-restore

docker run --name serv -p 5000:5000 -v /Users/AntonEfimenko/pgdata_back:/pgdata_back --link pgs2:db -e POSTGRES_PASSWORD=psw -e PGDATA_BACKUP=/pgdata_back/latest.psql.gz -e BACKUP_RESTORE_LOG=/pgdata_back/restore.log -e POSTGRES_USER=postgres -e POSTGRES_HOST=db -e POSTGRES_PORT=5432 -e POSTGRES_DB=postgres -e MIGRATE_LOG=/pgdata_back/migrate.log -i reloni/todo-service:dev-latest
