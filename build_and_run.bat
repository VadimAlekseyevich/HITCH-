@echo off
setlocal EnableExtensions EnableDelayedExpansion

cd /d "%~dp0"
set "ROOT=%CD%"

set "DOTNET_VERSION=8.0.425"
set "GODOT_VERSION=4.7.2"
set "TOOLS_DIR=%ROOT%\.tools"
set "DOTNET_LOCAL=%TOOLS_DIR%\dotnet"
set "GODOT_DIR=%TOOLS_DIR%\godot-%GODOT_VERSION%"
set "GODOT_EXE_NAME=Godot_v%GODOT_VERSION%-stable_mono_win64.exe"
set "GODOT_URL=https://downloads.godotengine.org/?flavor=stable&platform=windows.64&slug=mono_win64.zip&version=%GODOT_VERSION%"

set "MODE=%~1"
if "%MODE%"=="" set "MODE=run"

if /I not "%MODE%"=="build" if /I not "%MODE%"=="test" if /I not "%MODE%"=="run" goto :usage

echo.
echo ============================================================
echo   HITCH! bootstrap / build
echo   Mode: %MODE%
echo ============================================================
echo.

call :ensure_dotnet
if errorlevel 1 goto :fail

echo [HITCH] .NET:
"!DOTNET_CMD!" --version
if errorlevel 1 goto :fail

echo.
echo [HITCH] Restoring game project...
"!DOTNET_CMD!" restore "%ROOT%\HITCH.csproj"
if errorlevel 1 goto :fail

echo.
echo [HITCH] Building game project...
"!DOTNET_CMD!" build "%ROOT%\HITCH.csproj" -c Debug --no-restore
if errorlevel 1 goto :fail

if /I "%MODE%"=="build" goto :success

echo.
echo [HITCH] Restoring tests...
"!DOTNET_CMD!" restore "%ROOT%\tests\HITCH.Tests\HITCH.Tests.csproj"
if errorlevel 1 goto :fail

echo.
echo [HITCH] Running tests...
"!DOTNET_CMD!" test "%ROOT%\tests\HITCH.Tests\HITCH.Tests.csproj" -c Debug --no-restore
if errorlevel 1 goto :fail

if /I "%MODE%"=="test" goto :success

call :ensure_godot
if errorlevel 1 goto :fail

echo.
echo [HITCH] Starting Godot %GODOT_VERSION%...
echo [HITCH] Close the game window to return to this terminal.
echo.
"!GODOT_EXE!" --path "%ROOT%"
if errorlevel 1 goto :fail

goto :success


:ensure_dotnet
if exist "%DOTNET_LOCAL%\dotnet.exe" (
    set "DOTNET_CMD=%DOTNET_LOCAL%\dotnet.exe"
    set "DOTNET_ROOT=%DOTNET_LOCAL%"
    set "PATH=%DOTNET_LOCAL%;!PATH!"
    echo [HITCH] Using repository-local .NET SDK %DOTNET_VERSION%.
    exit /b 0
)

set "GLOBAL_DOTNET_VERSION="
where dotnet >nul 2>&1
if not errorlevel 1 (
    for /f "delims=" %%V in ('dotnet --version 2^>nul') do set "GLOBAL_DOTNET_VERSION=%%V"
)

if /I "!GLOBAL_DOTNET_VERSION!"=="%DOTNET_VERSION%" (
    set "DOTNET_CMD=dotnet"
    echo [HITCH] Using installed .NET SDK %DOTNET_VERSION%.
    exit /b 0
)

echo [HITCH] .NET SDK %DOTNET_VERSION% not found. Installing locally...
if not exist "%TOOLS_DIR%" mkdir "%TOOLS_DIR%"

set "HITCH_TOOLS_DIR=%TOOLS_DIR%"
set "HITCH_DOTNET_DIR=%DOTNET_LOCAL%"
set "HITCH_DOTNET_VERSION=%DOTNET_VERSION%"

powershell -NoProfile -ExecutionPolicy Bypass -Command "$ErrorActionPreference='Stop'; New-Item -ItemType Directory -Force -Path $env:HITCH_TOOLS_DIR | Out-Null; $installer=Join-Path $env:HITCH_TOOLS_DIR 'dotnet-install.ps1'; Invoke-WebRequest -UseBasicParsing 'https://dot.net/v1/dotnet-install.ps1' -OutFile $installer; & $installer -Version $env:HITCH_DOTNET_VERSION -InstallDir $env:HITCH_DOTNET_DIR -Architecture x64 -NoPath"
if errorlevel 1 exit /b 1

if not exist "%DOTNET_LOCAL%\dotnet.exe" (
    echo [HITCH] ERROR: .NET installation finished but dotnet.exe was not found.
    exit /b 1
)

set "DOTNET_CMD=%DOTNET_LOCAL%\dotnet.exe"
set "DOTNET_ROOT=%DOTNET_LOCAL%"
set "PATH=%DOTNET_LOCAL%;!PATH!"
exit /b 0


:ensure_godot
set "GODOT_EXE="
if exist "%GODOT_DIR%" (
    for /r "%GODOT_DIR%" %%F in ("%GODOT_EXE_NAME%") do (
        if not defined GODOT_EXE set "GODOT_EXE=%%~fF"
    )
)

if defined GODOT_EXE (
    echo [HITCH] Using repository-local Godot %GODOT_VERSION%.
    exit /b 0
)

echo.
echo [HITCH] Godot %GODOT_VERSION% .NET not found. Downloading locally...
if not exist "%TOOLS_DIR%" mkdir "%TOOLS_DIR%"

set "HITCH_TOOLS_DIR=%TOOLS_DIR%"
set "HITCH_GODOT_DIR=%GODOT_DIR%"
set "HITCH_GODOT_URL=%GODOT_URL%"

powershell -NoProfile -ExecutionPolicy Bypass -Command "$ErrorActionPreference='Stop'; $zip=Join-Path $env:HITCH_TOOLS_DIR 'godot-dotnet.zip'; if (Test-Path $env:HITCH_GODOT_DIR) { Remove-Item -Recurse -Force $env:HITCH_GODOT_DIR }; New-Item -ItemType Directory -Force -Path $env:HITCH_GODOT_DIR | Out-Null; Invoke-WebRequest -UseBasicParsing -Uri $env:HITCH_GODOT_URL -OutFile $zip; Expand-Archive -LiteralPath $zip -DestinationPath $env:HITCH_GODOT_DIR -Force; Remove-Item -Force $zip"
if errorlevel 1 exit /b 1

for /r "%GODOT_DIR%" %%F in ("%GODOT_EXE_NAME%") do (
    if not defined GODOT_EXE set "GODOT_EXE=%%~fF"
)

if not defined GODOT_EXE (
    echo [HITCH] ERROR: Godot archive was extracted but %GODOT_EXE_NAME% was not found.
    exit /b 1
)

exit /b 0


:usage
echo Usage:
echo   build_and_run.bat          Download prerequisites if needed, build, test, run.
echo   build_and_run.bat build    Download .NET if needed and build only.
echo   build_and_run.bat test     Download .NET if needed, build and run tests.
echo   build_and_run.bat run      Same as double-click/default mode.
exit /b 2


:success
echo.
echo [HITCH] Success.
exit /b 0


:fail
echo.
echo [HITCH] FAILED. See the error above.
echo.
pause
exit /b 1
