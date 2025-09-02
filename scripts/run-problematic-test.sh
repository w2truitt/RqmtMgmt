#!/bin/bash

# Quick test runner for the problematic ProjectsPageTests method
# This script specifically targets the test that's causing timeout issues

set -e

# Get the directory where this script is located
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(dirname "$SCRIPT_DIR")"
E2E_TESTS_DIR="$PROJECT_ROOT/frontend.E2ETests"

echo "🎯 Quick Test: Projects_HasExpectedPageElements_AuthenticatedUser"
echo "E2E Tests Dir: $E2E_TESTS_DIR"

# Change to E2E tests directory
cd "$E2E_TESTS_DIR"

# Set test-specific environment variables
export ASPNETCORE_ENVIRONMENT=Testing
export TEST_BASE_URL=https://rqmtmgmt.local

echo ""
echo "🧪 Running the problematic test with 5-minute timeout..."
echo "----------------------------------------"

# Run the specific test with timeout and better error handling
start_time=$(date +%s)

# Use timeout but handle it gracefully
set +e  # Temporarily disable exit on error
timeout 300 dotnet test \
    --configuration Release \
    --logger "console;verbosity=normal" \
    --logger "trx;LogFileName=TestResults-ProblematicTest.trx" \
    --filter "FullyQualifiedName~ProjectsPageTests.Projects_HasExpectedPageElements_AuthenticatedUser" \
    --collect:"XPlat Code Coverage" \
    --settings ../coverlet.runsettings

exit_code=$?
set -e  # Re-enable exit on error

end_time=$(date +%s)
duration=$((end_time - start_time))

echo ""
echo "📊 Test Results:"
echo "   Duration: ${duration} seconds"

if [ $exit_code -eq 0 ]; then
    echo "   Status: ✅ PASSED"
    echo "   The test is working correctly!"
elif [ $exit_code -eq 124 ]; then
    echo "   Status: ⏰ TIMED OUT"
    echo "   This confirms the test is causing timeout issues!"
    echo "   Recommendation: Investigate page load times and element visibility waits"
else
    echo "   Status: ❌ FAILED (exit code: $exit_code)"
    echo "   The test failed for reasons other than timeout"
fi

echo ""
echo "🏁 Quick test completed."