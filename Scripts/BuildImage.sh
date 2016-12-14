#!/bin/sh
#docker run -it -rm -d -v $PWD:/sources microsoft/dotnet:1.1.0-sdk-msbuild tail -f /dev/null
#docker run -it -d --name builder microsoft/dotnet:1.1.0-sdk-msbuild tail -f /dev/null
#docker cp src/simpletodoservice builder:app
#docker exec builder bash -c 'cd /app; dotnet restore; dotnet publish -o "../published/debug"'
#docker cp builder:published/debug published
set -ev

if [ "${TRAVIS_TAG}" != "" ]; then
  #docker run -it --rm -v $curdir:/sources microsoft/dotnet:1.1.0-sdk-msbuild bash /sources/scripts/PublishDebug.sh
  docker run -it -d --name builder microsoft/dotnet:1.1.0-sdk-msbuild tail -f /dev/null
  echo "try ls"
  docker exec builder bash -c 'ls'
  docker cp src/SimpleToDoService builder:app
  docker exec builder bash -c 'cd /app; dotnet restore; dotnet publish -o "../published/debug"'
  docker cp builder:published published
  docker login -u ${DOCKER_USER} -p ${DOCKER_PASS}
  export REPO=reloni/todo-serivce
  export TAG=Debug-${TRAVIS_TAG}
  docker build -f Dockerfile.debug -t $REPO:$TAG .
  docker tag $REPO:$TAG $REPO:latest
  docker push $REPO
fi

exit 0;
