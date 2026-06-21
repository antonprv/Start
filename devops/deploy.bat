@echo off
setlocal enabledelayedexpansion

set "PROJECT_NAME=%~1"
set "BUILD_MODE=%~2"
set "BUILD_SUBPATH=%~3"

if "%PROJECT_NAME%"=="" (
    echo Error: Project name not specified.
    echo Usage: %~nx0 ^<<ProjectName^> [BuildMode] [BuildSubpath]
    echo   BuildMode: debug or release. Default: debug
    echo   BuildSubpath: relative path inside project folder. Default: src\Builds\Build1
    exit /b 1
)

:: Default build mode if not provided
if /I "%BUILD_MODE%"=="release" (
    set "EXPORT_FLAG=--export-release"
    set "MODE_NAME=release"
) else if /I "%BUILD_MODE%"=="debug" (
    set "EXPORT_FLAG=--export-debug"
    set "MODE_NAME=debug"
) else (
    :: If BuildMode is not recognized, treat it as BuildSubpath and default to debug
    if not "%BUILD_MODE%"=="" (
        set "BUILD_SUBPATH=%BUILD_MODE%"
    )
    set "EXPORT_FLAG=--export-debug"
    set "MODE_NAME=debug"
)

:: Default build subpath if not provided
if "%BUILD_SUBPATH%"=="" set "BUILD_SUBPATH=src\Builds\Build1"

:: Base paths
set "GODOT_BASE=C:\Users\User\Godot"
set "EDITOR_PATH=%GODOT_BASE%\Editors\.editor_config\%PROJECT_NAME%\Godot_v4.7-stable_mono_win64_console.exe"
set "PROJECT_PATH=%GODOT_BASE%\Projects\%PROJECT_NAME%\src\%PROJECT_NAME%"
set "BUILD_BASE=%GODOT_BASE%\Projects\%PROJECT_NAME%\%BUILD_SUBPATH%"

:: Find the next available build number
set "BUILD_DIR=%BUILD_BASE%"
set "COUNTER=1"

:find_next
if exist "%BUILD_DIR%" (
    set /a COUNTER+=1
    set "BUILD_DIR=%BUILD_BASE:~0,-1%%COUNTER%"
    goto find_next
)

set "OUTPUT_PATH=%BUILD_DIR%\game.exe"

:: Create build directory recursively
if not exist "%BUILD_DIR%" (
    echo Creating build directory: %BUILD_DIR%
    powershell -Command "New-Item -ItemType Directory -Force -Path '%BUILD_DIR%'" >nul 2>&1
    if errorlevel 1 (
        echo Error: Failed to create directory %BUILD_DIR%
        exit /b 1
    )
)

:: Run export
echo Build mode: %MODE_NAME%
echo Exporting to: %OUTPUT_PATH%
"%EDITOR_PATH%" --path "%PROJECT_PATH%" %EXPORT_FLAG% "Windows Desktop" "%OUTPUT_PATH%"

if errorlevel 1 (
    echo Export failed.
    exit /b 1
)

echo Export completed successfully: %OUTPUT_PATH%