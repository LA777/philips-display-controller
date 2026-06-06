@echo off
setlocal EnableDelayedExpansion

rem ===========================================================================
rem  install.cmd - build and install pdc WITHOUT Visual Studio.
rem  Uses only the .NET 10 SDK (https://dotnet.microsoft.com/download).
rem
rem  It publishes a self-contained single-file pdc.exe, copies it to
rem  %LOCALAPPDATA%\Programs\pdc, and adds that folder to your user PATH.
rem ===========================================================================

set "ROOT=%~dp0"
set "CONFIG=Release"
set "PUBLISH=%ROOT%pdc\bin\%CONFIG%\net10.0-windows\win-x64\publish"
set "INSTALLDIR=%LOCALAPPDATA%\Programs\pdc"

echo.
echo === pdc installer ===
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

rem --- 4. Copy to the install directory ---
echo.
echo Installing to "%INSTALLDIR%"...
if not exist "%INSTALLDIR%" mkdir "%INSTALLDIR%"
copy /y "%PUBLISH%\pdc.exe" "%INSTALLDIR%\pdc.exe" >nul
if errorlevel 1 (
    echo ERROR: Failed to copy pdc.exe to "%INSTALLDIR%".
    goto fail
)

rem --- 5. Add the install dir to the USER PATH (only if missing) ---
set "USERPATH="
for /f "tokens=2,*" %%a in ('reg query "HKCU\Environment" /v Path 2^>nul') do set "USERPATH=%%b"

echo !USERPATH! | find /i "%INSTALLDIR%" >nul
if errorlevel 1 (
    if defined USERPATH (
        setx PATH "!USERPATH!;%INSTALLDIR%" >nul
    ) else (
        setx PATH "%INSTALLDIR%" >nul
    )
    echo Added "%INSTALLDIR%" to your user PATH.
    set "PATHCHANGED=1"
) else (
    echo "%INSTALLDIR%" is already on your PATH.
)

echo.
echo === Done ===
echo Installed: "%INSTALLDIR%\pdc.exe"
if defined PATHCHANGED echo NOTE: open a NEW terminal for the PATH change to take effect.
echo.
echo Try it:
echo     pdc --info
echo     pdc --brightness all:50
echo.
endlocal
exit /b 0

:fail
echo.
echo Installation failed.
endlocal
exit /b 1
