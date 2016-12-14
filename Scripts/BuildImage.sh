set -ev

echo ${TRAVIS_BUILD_DIR}
ls

if [ "${TRAVIS_TAG}" != "" ]; then
  docker run -it --rm -v /${TRAVIS_BUILD_DIR}/.:/sources microsoft/dotnet:1.1.0-sdk-msbuild bash /sources/scripts/PublishDebug.sh
  docker login -u ${DOCKER_USER} -p ${DOCKER_PASS}
  export REPO=reloni/todo-serivce
  export TAG=Debug-${TRAVIS_TAG}
  docker build -f Dockerfile.debug -t $REPO:$TAG .
  docker tag $REPO:$TAG $REPO:latest
  docker push $REPO
fi

exit 0;
