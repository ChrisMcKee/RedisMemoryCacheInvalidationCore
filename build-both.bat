@echo off
echo Building both signed and unsigned versions of RedisMemoryCacheInvalidationCore...

echo.
echo Building unsigned version...
call build-unsigned.bat

echo.
echo Building signed version...
call build-signed.bat

echo.
echo Both packages created in artifacts folder:
dir artifacts\*.nupkg
