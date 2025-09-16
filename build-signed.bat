@echo off
echo Building signed version of RedisMemoryCacheInvalidationCore...
dotnet build src\RedisMemoryCacheInvalidation\RedisMemoryCacheInvalidation.csproj -c Release -p:SignAssembly=true
dotnet pack src\RedisMemoryCacheInvalidation\RedisMemoryCacheInvalidation.csproj -c Release -p:SignAssembly=true --no-build -o artifacts
echo Signed package created in artifacts folder
