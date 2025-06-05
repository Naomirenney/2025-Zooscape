#!/usr/bin/env bash

# Default values
NUM_REFBOTS=3
MAX_TICKS=1000
TICK_DURATION=200
FULL_LOGS=false
DIFF_LOGS=false
WORLD_MAP="generate:51|6|0.0|0.5"
SEED=0

# Parse named arguments
while [[ "$#" -gt 0 ]]; do
    case $1 in
        --refbots) NUM_REFBOTS="$2"; shift ;;
        --maxticks) MAX_TICKS="$2"; shift ;;
        --tickduration) TICK_DURATION="$2"; shift ;;
        --map) WORLD_MAP="$2"; shift ;;
        --seed) SEED="$2"; shift ;;
        --full-logs) FULL_LOGS=true ;;
        --diff-logs) DIFF_LOGS=true ;;
        *) echo "Unknown parameter passed: $1"; exit 1 ;;
    esac
    shift
done

# Export variables for docker-compose
export NUM_REFBOTS
export MAX_TICKS
export TICK_DURATION
export FULL_LOGS
export DIFF_LOGS
export WORLD_MAP
export SEED

# Stop and remove containers if they are running
docker compose down --volumes
echo "Purging logs..."
rm -f logs/*

echo "Building docker images..."
docker compose build

echo "Running engine and $NUM_REFBOTS reference bots"
docker compose up -d --scale refbot=$NUM_REFBOTS
ENGINE_NAME=$(docker compose ps -q engine)

docker compose logs --follow engine

# Wait for the 'engine' container to stop
docker wait $ENGINE_NAME

# Stop and remove containers
docker compose down --volumes