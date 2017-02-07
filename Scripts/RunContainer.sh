docker run -d --name task-manager-service -p 5000:5000 -e POSTGRES_PASSWORD=test -e POSTGRES_USER=taskmanager -e POSTGRES_DB=tasksdb -e POSTGRES_HOST=localhost -e POSTGRES_PORT=5432 reloni/todo-service:latest


docker run -d --name task-manager-service -p 5000:5000 -e POSTGRES_PASSWORD=test -e POSTGRES_USER=taskmanager -e POSTGRES_DB=tasksdb -e POSTGRES_HOST=localhost -e POSTGRES_PORT=5432 test-todo
