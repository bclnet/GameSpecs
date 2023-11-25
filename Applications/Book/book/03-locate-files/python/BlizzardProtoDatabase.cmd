@echo off
echo build BlizzardProtoDatabase.proto
protoc -I ..\..\..\..\..\Context\Gamespec.StoreManager\StoreManagers --python_out=. BlizzardProtoDatabase.proto