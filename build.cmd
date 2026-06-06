@echo off
setlocal

rem ===========================================================================
rem  build.cmd - build pdc WITHOUT Visual Studio, using only the .NET SDK.
rem  Produces a self-contained, single-file pdc.exe. Does NOT install or
rem  modify PATH (use install.cmd for that).
rem ===========================================================================

set "ROOT=%~dp0"
set "CONFIG=Release"
set "PUBLISH=%ROOT%pdc\bin\%CONFIG%\net10.0-windows\win-x64\publish"

echo.
echo === pdc build ===
echo.

rem --- 1. Is the .NET SDK available? ---
where dotnet >nul 2>&1
if errorlevel 1 (
    echo ERROR: The .NET SDK was not found on PATH.
    echo Install the .NET 10 SDK from https://dotnet.microsoft.com/download
    echo then run this script again.
    goto fail
)

rem --- 2. Is a .NET 10 SDK installed? (the projects target net10.0) ---
set "HAS10="
for /f "tokens=1 delims=. " %%v in ('dotnet --list-sdks') do (
    if "%%v"=="10" set "HAS10=1"
)
if not defined HAS10 (
    echo ERROR: No .NET 10 SDK found. Installed SDKs:
    dotnet --list-sdks
    echo Install the .NET 10 SDK from https://dotnet.microsoft.com/download
    goto fail
)

rem --- 3. Build: publish a self-contained, single-file executable ---
echo Building pdc ^(%CONFIG%, self-contained single file^)...
echo.
dotnet publish "%ROOT%pdc\pdc.csproj" -c %CONFIG%
if errorlevel 1 (
    echo.
    echo ERROR: Build failed.
    goto fail
)
if not exist "%PUBLISH%\pdc.exe" (
    echo ERROR: Expected output not found: "%PUBLISH%\pdc.exe"
    goto fail
)

echo.
echo === Build succeeded ===
echo Output: "%PUBLISH%\pdc.exe"
echo.
echo Run it directly, e.g.:
echo     "%PUBLISH%\pdc.exe" --info
echo.
echo To install it onto your PATH, run install.cmd instead.
echo.
endlocal
exit /b 0

:fail
echo.
echo Build failed.
endlocal
exit /b 1
