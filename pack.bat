echo off
SET VERSION=%1
IF [%VERSION%]==[] SET VERSION=2.2.0-local

dotnet build -c Release RedisMemoryCacheInvalidation.sln
dotnet pack RedisMemoryCacheInvalidation.sln -c Release --include-symbols --nologo -o artifacts/ -p:PackageVersion=%VERSION%
