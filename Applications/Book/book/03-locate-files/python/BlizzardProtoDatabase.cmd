@echo off
echo build BlizzardProtoDatabase.proto
rem protoc -I . --pyi_out=. BlizzardProtoDatabase.proto
protoc -I ..\..\..\..\..\Context\Gamespec.StoreManager\StoreManagers --python_out=. BlizzardProtoDatabase.proto