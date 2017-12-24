#!/bin/bash

set -e

export DOTNETSDK="2.0.4-sdk-2.1.3-stretch"

docker run -it -d --name builder microsoft/dotnet:"$DOTNETSDK" tail -f /dev/null

docker exec builder bash -c 'mkdir -p app/SimpleToDoService; exit $?'
docker exec builder bash -c 'mkdir -p app/SimpleToDoServiceTests; exit $?'
docker cp src/SimpleToDoService/. builder:app/SimpleToDoService
docker cp src/SimpleToDoServiceTests/. builder:app/SimpleToDoServiceTests

# run tests
docker exec builder bash -c 'cd /app/SimpleToDoServiceTests; dotnet restore; dotnet test ./SimpleToDoServiceTests.csproj; exit $?'

if [ "${TRAVIS_TAG}" != "" ]; then
  # if tag doesn't contain word "beta" it's release build
  if [[ "$TRAVIS_TAG" != *"beta"* ]]; then
    export SUBTAG=release
  else
    export SUBTAG=develop
  fi

  export REPO=${DOCKER_AWS_REPONAME}
  export TAG=${TRAVIS_TAG}-$SUBTAG

  if [ "$SUBTAG" = "release" ]; then
    docker exec builder bash -c 'cd /app/SimpleToDoService; dotnet restore; dotnet publish --configuration release -o "../../published/release"; exit $?'
  else
    docker exec builder bash -c 'cd /app/SimpleToDoService; dotnet restore; dotnet publish --configuration debug -o "../../published/debug"; exit $?'
  fi
  docker cp builder:published published

  if [ "$SUBTAG" = "release" ]; then
    export DOCKERFILE=Dockerfile.release
  else
    export DOCKERFILE=Dockerfile.debug
  fi

  #push to AWS
  aws ecr get-login --no-include-email --region ${DOCKER_AWS_REGION} > login
  eval "$(cat login)"
  docker build -f $DOCKERFILE -t $REPO:$TAG .

  if [ "$SUBTAG" = "release" ]; then
    docker tag $REPO:$TAG $REPO:latest
  fi
  docker tag $REPO:$TAG $REPO:dev-latest

  docker push $REPO > PushLog.log
  echo "AWS push log ===="
  cat PushLog.log
  echo "======"

  #push to docker-hub
  docker login -u ${DOCKER_USER} -p ${DOCKER_PASS}
  export REPO=reloni/todo-service
  docker build -f $DOCKERFILE -t $REPO:$TAG .

  if [ "$SUBTAG" = "release" ]; then
    docker tag $REPO:$TAG $REPO:latest
  fi
  docker tag $REPO:$TAG $REPO:dev-latest

  docker push $REPO > PushLog.log
  echo "Docker hub push log ===="
  cat PushLog.log
  echo "======"

  # restart development containers
  TASKARN=$(aws ecs list-tasks --cluster ${DOCKER_AWS_CLUSTER_NAME} --region ${DOCKER_AWS_REGION} --service-name ${DOCKER_AWS_DEV_TASK_SERVICE} --output text | cut -d$'\t' -f2)
  aws ecs stop-task --task $TASKARN --cluster ${DOCKER_AWS_CLUSTER_NAME} --region ${DOCKER_AWS_REGION}
fi

exit 0;
