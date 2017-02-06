#!/bin/sh
set -e

if [ "${TRAVIS_TAG}" != "" ]; then

  docker run -it -d --name builder microsoft/dotnet:1.1.0-sdk-msbuild tail -f /dev/null
  docker cp src/SimpleToDoService builder:app
  docker exec builder bash -c 'cd /app; dotnet restore; dotnet publish -o "../published/debug"'
  docker cp builder:published published

  #push to AWS
  aws ecr get-login --region eu-central-1 > login
  eval "$(cat login)"
  export REPO=${DOCKER_AWS_REPONAME}
  export TAG=empty-${TRAVIS_TAG}
  docker build -f Dockerfile.empty -t $REPO:$TAG --label Postgres=${DBVersion} .
  docker tag $REPO:$TAG $REPO:latest
  docker push $REPO > PushLog.log
  echo "AWS push log ===="
  cat PushLog.log
  echo "======"

  #push to docker-hub
  docker login -u ${DOCKER_USER} -p ${DOCKER_PASS}
  export REPO=reloni/todo-service
  docker build -f Dockerfile.empty -t $REPO:$TAG --label Postgres=${DBVersion} .
  docker tag $REPO:$TAG $REPO:latest
  docker push $REPO > PushLog.log
  echo "Docker hub push log ===="
  cat PushLog.log
  echo "======"
fi

exit 0;
