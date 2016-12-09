mkdir -p ./Publish
(cd ./src/SimpleToDoService && dotnet publish -o "../../Publish/Debug" -c Debug)
