#!/bin/bash

# E2E Test Runner with Logging Control
# Usage: ./run-e2e-tests.sh [log-level] [test-filter]
# Log levels: Silent, Minimal, Normal, Detailed, Verbose
# Examples:
#   ./run-e2e-tests.sh Minimal                    # Run all tests with minimal logging
#   ./run-e2e-tests.sh Normal "*Authentication*"  # Run authentication tests with normal logging
#   ./run-e2e-tests.sh Detailed                   # Run all tests with detailed logging

LOG_LEVEL=${1:-Normal}
TEST_FILTER=${2:-""}

echo "Running E2E tests with log level: $LOG_LEVEL"

# Set environment variable for test logger
export XUNIT_LOG_LEVEL=$LOG_LEVEL

# Build the dotnet test command
CMD="dotnet test"

# Add test filter if provided
if [ ! -z "$TEST_FILTER" ]; then
    CMD="$CMD --filter \"$TEST_FILTER\""
fi

# Add logger configuration based on log level
case $LOG_LEVEL in
    "Silent")
        CMD="$CMD --logger=\"console;verbosity=quiet\" --verbosity quiet"
        ;;
    "Minimal")
        CMD="$CMD --logger=\"console;verbosity=minimal\" --verbosity minimal"
        ;;
    "Normal")
        CMD="$CMD --logger=\"console;verbosity=normal\" --verbosity normal"
        ;;
    "Detailed")
        CMD="$CMD --logger=\"console;verbosity=detailed\" --verbosity detailed"
        ;;
    "Verbose")
        CMD="$CMD --logger=\"console;verbosity=diagnostic\" --verbosity diagnostic"
        ;;
    *)
        echo "Unknown log level: $LOG_LEVEL"
        echo "Valid levels: Silent, Minimal, Normal, Detailed, Verbose"
        exit 1
        ;;
esac

echo "Executing: $CMD"
eval $CMD