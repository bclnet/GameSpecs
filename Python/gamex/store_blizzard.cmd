@echo off
echo build Blizzard.proto
protoc -I ..\..\Context\Gamespec.Base\StoreManagers --python_out=. Blizzard.proto