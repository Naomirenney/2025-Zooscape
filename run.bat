@echo off
setlocal enabledelayedexpansion

REM Set default values
set NUM_REFBOTS=3
set MAX_TICKS=1000
set TICK_DURATION=200
set FULL_LOGS=false
set DIFF_LOGS=false
set SEED=0

REM Parse named arguments
:parse_args
if "%~1"=="" goto after_args
if "%~1"=="--refbots" (
    set NUM_REFBOTS=%~2
    shift
) else if "%~1"=="--maxticks" (
    set MAX_TICKS=%~2
    shift
) else if "%~1"=="--tickduration" (
    set TICK_DURATION=%~2
    shift
) else if "%~1"=="--seed" (
    set SEED=%~2
    shift
) else if "%~1"=="--full-logs" (
    set "FULL_LOGS=true"
) else if "%~1"=="--diff-logs" (
    set "DIFF_LOGS=true"
) else (
    echo Unknown parameter: %~1
    exit /b 1
)
shift
goto parse_args

:after_args

REM Write environment variables to .env file
(
    echo NUM_REFBOTS=%NUM_REFBOTS%
    echo MAX_TICKS=%MAX_TICKS%
    echo TICK_DURATION=%TICK_DURATION%    
    echo FULL_LOGS=%FULL_LOGS%
    echo DIFF_LOGS=%DIFF_LOGS%
    echo SEED=%SEED%
) > .env

REM Stop and remove containers if they are running
docker compose down --volumes

echo "Purging logs..."
rd /s /q logs

echo "Building docker images..."
docker compose build

echo "Running engine and %numbots% reference bots"
docker compose up -d --scale refbot=%NUM_REFBOTS%
docker compose logs --follow engine

REM Wait for the 'engine' container to stop
FOR /F "usebackq delims=" %%i IN (`docker compose ps -q engine`) DO docker wait %%i

REM Stop and remove containers
docker compose down --volumes