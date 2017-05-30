#!/bin/sh
set -e

if [ "${TRAVIS_TAG}" != "" ]; then
  # if tag doesn't contain word "beta" it's release build
  if [[ "$TRAVIS_TAG" != *"beta"* ]]; then
    export SUBTAG=release
  else
    export SUBTAG=develop
  fi

  docker run -it -d --name builder microsoft/dotnet:1.1.2-sdk tail -f /dev/null
  docker cp src/SimpleToDoService builder:app
  docker exec builder bash -c 'cd /app; dotnet restore; dotnet publish --configuration release -o "../published/release"'
  docker cp builder:published published

  #push to AWS
  aws ecr get-login --no-include-email --region eu-central-1 > login
  eval "$(cat login)"
  export REPO=${DOCKER_AWS_REPONAME}
  export TAG=${TRAVIS_TAG}-release
  docker build -f Dockerfile.release -t $REPO:$TAG .
  docker tag $REPO:$TAG $REPO:latest
  docker push $REPO > PushLog.log
  echo "AWS push log ===="
  cat PushLog.log
  echo "======"

  #push to docker-hub
  docker login -u ${DOCKER_USER} -p ${DOCKER_PASS}
  export REPO=reloni/todo-service
  docker build -f Dockerfile.release -t $REPO:$TAG .
  docker tag $REPO:$TAG $REPO:latest
  docker push $REPO > PushLog.log
  echo "Docker hub push log ===="
  cat PushLog.log
  echo "======"
fi

exit 0;
