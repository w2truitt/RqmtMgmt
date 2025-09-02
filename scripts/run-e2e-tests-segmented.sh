#!/bin/bash

# Segmented E2E Test Runner
# This script runs E2E tests in smaller groups to identify and troubleshoot issues

set -e

# Get the directory where this script is located
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(dirname "$SCRIPT_DIR")"
E2E_TESTS_DIR="$PROJECT_ROOT/frontend.E2ETests"

echo "🧪 RqmtMgmt Segmented E2E Test Runner"
echo "Project Root: $PROJECT_ROOT"
echo "E2E Tests Dir: $E2E_TESTS_DIR"

# Change to E2E tests directory
cd "$E2E_TESTS_DIR"

# Set test-specific environment variables
export ASPNETCORE_ENVIRONMENT=Testing
export TEST_BASE_URL=https://rqmtmgmt.local

# Define test segments
declare -A TEST_SEGMENTS=(
    ["smoke"]="SmokeTests"
    ["integration"]="IntegrationTests"
    ["authentication"]="AuthenticationDiagnosticTests,AuthenticatedWorkflowTests,JwtTokenEmailExtractionTests,BackendEmailExtractionWorkflowTests"
    ["basic-navigation"]="DashboardPageTests,ProjectsPageTests,UsersPageTests"
    ["project-management"]="ProjectNavigationE2ETests,ProjectSelectionWorkflowTests,BasicProjectSelectionTests,ProjectRequirementsE2ETests"
    ["user-management"]="UserRoleManagementE2ETests,UserManagementWorkflowTests,RoleAssignmentValidationTests"
    ["requirements"]="RequirementsWorkflowTests,ProjectRequirementsFilterTest,DebugRequirementsCreationTests"
    ["test-management"]="TestCasesPageTests,TestSuitesPageTests,TestPlansPageTests,TestRunSessionsPageTests,TestManagementWorkflowTests"
    ["debug-tests"]="DebugUsersPageStructure,DebugProjectPageElements,DebugProjectSelectorTests,DebugTestPlansPageTests"
    ["password-validation"]="PMPasswordValidationTests"
)

# Function to run a test segment
run_test_segment() {
    local segment_name=$1
    local test_classes=$2
    
    echo ""
    echo "🔍 Running $segment_name tests..."
    echo "Classes: $test_classes"
    echo "----------------------------------------"
    
    # Create filter for the test classes
    local filter=""
    IFS=',' read -ra CLASSES <<< "$test_classes"
    for class in "${CLASSES[@]}"; do
        if [ -z "$filter" ]; then
            filter="FullyQualifiedName~$class"
        else
            filter="$filter|FullyQualifiedName~$class"
        fi
    done
    
    # Run the tests with timeout and better error handling
    local start_time=$(date +%s)
    
    # Use timeout but handle it gracefully to avoid shell exit
    set +e  # Temporarily disable exit on error
    timeout 600 dotnet test \
        --configuration Release \
        --logger "console;verbosity=normal" \
        --logger "trx;LogFileName=TestResults-$segment_name.trx" \
        --filter "$filter" \
        --collect:"XPlat Code Coverage" \
        --settings ../coverlet.runsettings
    
    local exit_code=$?
    set -e  # Re-enable exit on error
    
    local end_time=$(date +%s)
    local duration=$((end_time - start_time))
    
    if [ $exit_code -eq 0 ]; then
        echo "✅ $segment_name tests PASSED (${duration}s)"
    elif [ $exit_code -eq 124 ]; then
        echo "⏰ $segment_name tests TIMED OUT (10 minutes, ${duration}s)"
        echo "   Consider running individual tests in this segment to isolate the issue"
        echo "   Use: ./run-projects-page-tests-individual.sh (for ProjectsPageTests)"
    else
        echo "❌ $segment_name tests FAILED (exit code: $exit_code, ${duration}s)"
    fi
    
    echo ""
    return $exit_code
}

# Function to show menu
show_menu() {
    echo ""
    echo "Available test segments:"
    echo "1) smoke - Basic smoke tests"
    echo "2) integration - Integration tests"
    echo "3) authentication - Authentication and JWT tests"
    echo "4) basic-navigation - Dashboard, Projects, Users page tests"
    echo "5) project-management - Project navigation and requirements"
    echo "6) user-management - User role management tests"
    echo "7) requirements - Requirements workflow tests"
    echo "8) test-management - Test cases, suites, plans, runs"
    echo "9) debug-tests - Debug and diagnostic tests"
    echo "10) password-validation - Password validation tests"
    echo "11) all - Run all segments sequentially"
    echo "12) custom - Enter custom test class names"
    echo "13) projects-individual - Run ProjectsPageTests individually"
    echo ""
}

# Main execution
if [ $# -eq 0 ]; then
    show_menu
    read -p "Enter your choice (1-13): " choice
else
    choice=$1
fi

case $choice in
    1|smoke)
        run_test_segment "smoke" "${TEST_SEGMENTS[smoke]}"
        ;;
    2|integration)
        run_test_segment "integration" "${TEST_SEGMENTS[integration]}"
        ;;
    3|authentication)
        run_test_segment "authentication" "${TEST_SEGMENTS[authentication]}"
        ;;
    4|basic-navigation)
        run_test_segment "basic-navigation" "${TEST_SEGMENTS[basic-navigation]}"
        ;;
    5|project-management)
        run_test_segment "project-management" "${TEST_SEGMENTS[project-management]}"
        ;;
    6|user-management)
        run_test_segment "user-management" "${TEST_SEGMENTS[user-management]}"
        ;;
    7|requirements)
        run_test_segment "requirements" "${TEST_SEGMENTS[requirements]}"
        ;;
    8|test-management)
        run_test_segment "test-management" "${TEST_SEGMENTS[test-management]}"
        ;;
    9|debug-tests)
        run_test_segment "debug-tests" "${TEST_SEGMENTS[debug-tests]}"
        ;;
    10|password-validation)
        run_test_segment "password-validation" "${TEST_SEGMENTS[password-validation]}"
        ;;
    11|all)
        echo "🚀 Running all test segments sequentially..."
        total_failed=0
        
        for segment in smoke integration basic-navigation project-management user-management requirements test-management authentication debug-tests password-validation; do
            run_test_segment "$segment" "${TEST_SEGMENTS[$segment]}"
            if [ $? -ne 0 ]; then
                ((total_failed++))
            fi
            
            # Brief pause between segments to avoid resource conflicts
            echo "⏳ Pausing 10 seconds between segments..."
            sleep 10
        done
        
        echo ""
        echo "📊 Summary: $total_failed segment(s) failed"
        if [ $total_failed -eq 0 ]; then
            echo "🎉 All segments passed!"
        else
            echo "⚠️  Some segments failed. Check individual results above."
        fi
        ;;
    12|custom)
        echo ""
        read -p "Enter test class names (comma-separated): " custom_classes
        run_test_segment "custom" "$custom_classes"
        ;;
    13|projects-individual)
        echo "🔄 Switching to individual ProjectsPageTests runner..."
        exec "./run-projects-page-tests-individual.sh"
        ;;
    *)
        echo "❌ Invalid choice. Please run the script again and choose 1-13."
        exit 1
        ;;
esac

echo ""
echo "🏁 Test execution completed."