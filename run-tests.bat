@echo off
REM Parallel Test Execution Helper Script for Windows
REM Usage: run-tests.bat [category]
REM Example: run-tests.bat Movement

setlocal enabledelayedexpansion

REM Get the project root directory
for %%I in (%CD%) do set PROJECT_DIR=%%~dpI

REM Check if Unity is installed (common locations)
set UNITY_PATH=
if exist "C:\Program Files\Unity\Hub\Editor\*\Editor\Unity.exe" (
    for /d %%D in ("C:\Program Files\Unity\Hub\Editor\*\Editor") do set UNITY_PATH=%%D\Unity.exe
)

if not exist "%UNITY_PATH%" (
    echo Error: Unity not found at %UNITY_PATH%
    echo Please specify Unity installation path manually.
    exit /b 1
)

REM Default category if not provided
set CATEGORY=%1
if "%CATEGORY%"=="" (
    echo Usage: run-tests.bat [category]
    echo Available categories: Movement, Attack, Enemy, Life, Item, UI
    echo Example: run-tests.bat Movement
    exit /b 1
)

REM Validate category
if /i not "%CATEGORY%"=="Movement" if /i not "%CATEGORY%"=="Attack" if /i not "%CATEGORY%"=="Enemy" if /i not "%CATEGORY%"=="Life" if /i not "%CATEGORY%"=="Item" if /i not "%CATEGORY%"=="UI" (
    echo Invalid category: %CATEGORY%
    echo Valid categories: Movement, Attack, Enemy, Life, Item, UI
    exit /b 1
)

echo.
echo Running %CATEGORY% tests...
echo.

REM Run tests with category filter
"%UNITY_PATH%" -projectPath "%PROJECT_DIR%" -runTests -testMode playmode -testCategory %CATEGORY% -logFile -

if %errorlevel% neq 0 (
    echo.
    echo Tests failed with exit code %errorlevel%
    exit /b %errorlevel%
) else (
    echo.
    echo %CATEGORY% tests completed successfully
)
