echo off
SET VERSION=%1
IF [%VERSION%]==[] SET VERSION=2.4.0-local

dotnet build -c Release RedisMemoryCacheInvalidation.sln
dotnet pack src\RedisMemoryCacheInvalidationCore\RedisMemoryCacheInvalidationCore.csproj -c Release --include-symbols --nologo -o artifacts/ -p:PackageVersion=%VERSION%
