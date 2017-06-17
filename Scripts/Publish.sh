#!/bin/bash

set -e

if [ "${TRAVIS_TAG}" != "" ]; then
  # if tag doesn't contain word "beta" it's release build
  if [[ "$TRAVIS_TAG" != *"beta"* ]]; then
    export SUBTAG=release
  else
    export SUBTAG=develop
  fi

  export REPO=${DOCKER_AWS_REPONAME}
  export TAG=${TRAVIS_TAG}-$SUBTAG

  docker run -it -d --name builder microsoft/dotnet:1.1.2-sdk tail -f /dev/null
  docker cp src/SimpleToDoService builder:app
  if [ "$SUBTAG" = "release" ]; then
    docker exec builder bash -c 'cd /app; dotnet restore; dotnet publish --configuration release -o "../published/release"'
  else
    docker exec builder bash -c 'cd /app; dotnet restore; dotnet publish --configuration debug -o "../published/debug"'
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
fi

exit 0;