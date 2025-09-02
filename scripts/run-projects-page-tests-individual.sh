#!/bin/bash

# Individual ProjectsPageTests Runner
# This script runs each ProjectsPageTests method individually to isolate timeout issues

set -e

# Get the directory where this script is located
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(dirname "$SCRIPT_DIR")"
E2E_TESTS_DIR="$PROJECT_ROOT/frontend.E2ETests"

echo "🔍 ProjectsPageTests Individual Test Runner"
echo "Project Root: $PROJECT_ROOT"
echo "E2E Tests Dir: $E2E_TESTS_DIR"

# Change to E2E tests directory
cd "$E2E_TESTS_DIR"

# Set test-specific environment variables
export ASPNETCORE_ENVIRONMENT=Testing
export TEST_BASE_URL=https://rqmtmgmt.local

# Define individual ProjectsPageTests methods
declare -a PROJECTS_PAGE_TESTS=(
    "Projects_NavigatesSuccessfully_AuthenticatedUser"
    "Projects_LoadsWithoutErrors_AuthenticatedUser"
    "Projects_HasExpectedPageElements_AuthenticatedUser"
    "Projects_CanCreateNewProject_AuthenticatedAdmin"
    "Projects_CanSearchProjects_AuthenticatedUser"
    "Projects_CanOpenAndCancelForm_AuthenticatedUser"
    "Projects_FormValidatesRequiredFields_AuthenticatedUser"
    "Projects_CanEditExistingProject_AuthenticatedAdmin"
    "Projects_CanDeleteProject_AuthenticatedAdmin"
    "Projects_CanPerformFullCrudWorkflow_AuthenticatedAdmin"
    "Projects_ShowsProjectCounts_AuthenticatedUser"
)

# Function to run a single test method
run_single_test() {
    local test_method=$1
    
    echo ""
    echo "🧪 Running test: $test_method"
    echo "----------------------------------------"
    
    # Create filter for the specific test method
    local filter="FullyQualifiedName~ProjectsPageTests.$test_method"
    
    # Run the test with timeout and better error handling
    local start_time=$(date +%s)
    
    # Use timeout but handle it gracefully to avoid shell exit
    set +e  # Temporarily disable exit on error
    timeout 300 dotnet test \
        --configuration Release \
        --logger "console;verbosity=normal" \
        --logger "trx;LogFileName=TestResults-$test_method.trx" \
        --filter "$filter" \
        --collect:"XPlat Code Coverage" \
        --settings ../coverlet.runsettings
    
    local exit_code=$?
    set -e  # Re-enable exit on error
    
    local end_time=$(date +%s)
    local duration=$((end_time - start_time))
    
    if [ $exit_code -eq 0 ]; then
        echo "✅ $test_method PASSED (${duration}s)"
        return 0
    elif [ $exit_code -eq 124 ]; then
        echo "⏰ $test_method TIMED OUT after 5 minutes (${duration}s)"
        echo "   This test is causing the timeout issue!"
        return 124
    else
        echo "❌ $test_method FAILED (exit code: $exit_code, ${duration}s)"
        return $exit_code
    fi
}

# Function to show menu
show_menu() {
    echo ""
    echo "Available ProjectsPageTests methods:"
    for i in "${!PROJECTS_PAGE_TESTS[@]}"; do
        echo "$((i+1))) ${PROJECTS_PAGE_TESTS[$i]}"
    done
    echo "$((${#PROJECTS_PAGE_TESTS[@]}+1))) all - Run all tests individually"
    echo "$((${#PROJECTS_PAGE_TESTS[@]}+2))) problematic - Run the problematic test (Projects_HasExpectedPageElements_AuthenticatedUser)"
    echo ""
}

# Main execution
if [ $# -eq 0 ]; then
    show_menu
    read -p "Enter your choice (1-$((${#PROJECTS_PAGE_TESTS[@]}+2))): " choice
else
    choice=$1
fi

# Handle the choice
if [[ "$choice" =~ ^[0-9]+$ ]] && [ "$choice" -ge 1 ] && [ "$choice" -le "${#PROJECTS_PAGE_TESTS[@]}" ]; then
    # Run individual test
    test_index=$((choice-1))
    run_single_test "${PROJECTS_PAGE_TESTS[$test_index]}"
elif [ "$choice" == "$((${#PROJECTS_PAGE_TESTS[@]}+1))" ] || [ "$choice" == "all" ]; then
    # Run all tests individually
    echo "🚀 Running all ProjectsPageTests individually..."
    total_failed=0
    total_timeout=0
    total_passed=0
    
    for test_method in "${PROJECTS_PAGE_TESTS[@]}"; do
        run_single_test "$test_method"
        exit_code=$?
        
        if [ $exit_code -eq 0 ]; then
            ((total_passed++))
        elif [ $exit_code -eq 124 ]; then
            ((total_timeout++))
            ((total_failed++))
        else
            ((total_failed++))
        fi
        
        # Brief pause between tests to avoid resource conflicts
        echo "⏳ Pausing 5 seconds between tests..."
        sleep 5
    done
    
    echo ""
    echo "📊 Summary:"
    echo "   ✅ Passed: $total_passed"
    echo "   ❌ Failed: $((total_failed - total_timeout))"
    echo "   ⏰ Timed out: $total_timeout"
    echo ""
    
    if [ $total_timeout -gt 0 ]; then
        echo "⚠️  Tests that timed out are likely causing the shell exit issue!"
    fi
    
    if [ $total_failed -eq 0 ]; then
        echo "🎉 All tests passed!"
    else
        echo "⚠️  Some tests failed or timed out. Check individual results above."
    fi
elif [ "$choice" == "$((${#PROJECTS_PAGE_TESTS[@]}+2))" ] || [ "$choice" == "problematic" ]; then
    # Run the problematic test specifically
    echo "🎯 Running the problematic test that was mentioned..."
    run_single_test "Projects_HasExpectedPageElements_AuthenticatedUser"
else
    echo "❌ Invalid choice. Please run the script again and choose a valid option."
    exit 1
fi

echo ""
echo "🏁 Individual test execution completed."