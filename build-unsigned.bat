@echo off
echo Building unsigned version of RedisMemoryCacheInvalidationCore...
dotnet build src\RedisMemoryCacheInvalidation\RedisMemoryCacheInvalidation.csproj -c Release
dotnet pack src\RedisMemoryCacheInvalidation\RedisMemoryCacheInvalidation.csproj -c Release --no-build -o artifacts
echo Unsigned package created in artifacts folder
