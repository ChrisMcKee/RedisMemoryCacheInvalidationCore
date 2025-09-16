echo off
SET VERSION=%1
IF [%VERSION%]==[] SET VERSION=2.5.0-local

echo Building unsigned version...
dotnet build src\RedisMemoryCacheInvalidation\RedisMemoryCacheInvalidation.csproj -c Release
dotnet pack src\RedisMemoryCacheInvalidation\RedisMemoryCacheInvalidation.csproj -c Release --include-symbols --nologo -o artifacts/ -p:PackageVersion=%VERSION%

echo Building signed version...
dotnet build src\RedisMemoryCacheInvalidation\RedisMemoryCacheInvalidation.csproj -c Release -p:SignAssembly=true
dotnet pack src\RedisMemoryCacheInvalidation\RedisMemoryCacheInvalidation.csproj -c Release -p:SignAssembly=true --include-symbols --nologo -o artifacts/ -p:PackageVersion=%VERSION%

echo Both packages created in artifacts folder
