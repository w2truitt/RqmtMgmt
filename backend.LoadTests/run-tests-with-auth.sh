#!/bin/bash

# Enhanced K6 Load Testing Suite Runner with Authentication and Output Management
# Provides proper environment variable handling and file-based output

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
PURPLE='\033[0;35m'
CYAN='\033[0;36m'
NC='\033[0m' # No Color

# Configuration
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
TESTS_DIR="$SCRIPT_DIR/tests"
RESULTS_DIR="$SCRIPT_DIR/load-test-results"

# Ensure results directory exists
mkdir -p "$RESULTS_DIR"

# Function to print colored output
print_header() {
    echo -e "${BLUE}=================================${NC}"
    echo -e "${BLUE}  K6 Load Testing Suite${NC}"
    echo -e "${BLUE}  Requirements Management API${NC}"
    echo -e "${BLUE}  WITH AUTHENTICATION${NC}"
    echo -e "${BLUE}=================================${NC}"
    echo ""
}

print_status() {
    echo -e "${GREEN}[INFO]${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}[WARN]${NC} $1"
}

print_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Function to load environment variables
load_env_vars() {
    if [[ -f "$SCRIPT_DIR/.env" ]]; then
        print_status "Loading environment variables from .env file..."
        export $(grep -v '^#' "$SCRIPT_DIR/.env" | grep -v '^$' | xargs)
        print_status "Environment variables loaded"
    else
        print_warning ".env file not found, using defaults"
    fi
}

# Function to check if K6 is installed
check_k6_installation() {
    if ! command -v k6 &> /dev/null; then
        print_error "K6 is not installed!"
        echo ""
        echo "Please install K6:"
        echo "  Ubuntu/Debian: sudo apt-get install k6"
        echo "  macOS: brew install k6"
        echo "  Windows: winget install k6"
        echo "  Docker: docker pull grafana/k6:latest"
        echo ""
        echo "Or visit: https://k6.io/docs/get-started/installation/"
        exit 1
    fi
    
    local k6_version=$(k6 version | head -n1)
    print_status "K6 detected: $k6_version"
}

# Function to check API availability
check_api_availability() {
    print_status "Checking API availability..."
    
    local api_url="${API_BASE_URL:-https://rqmtmgmt.local}"
    local health_endpoint="$api_url/health"
    
    if curl -s -k --max-time 10 "$health_endpoint" > /dev/null 2>&1; then
        print_status "API is available at $api_url"
        return 0
    else
        print_warning "API may not be available at $api_url"
        print_warning "Ensure containers are running: docker-compose -f docker-compose.yml -f docker-compose.identity.yml up -d"
        echo ""
        read -p "Continue anyway? (y/N): " -n 1 -r
        echo ""
        if [[ ! $REPLY =~ ^[Yy]$ ]]; then
            exit 1
        fi
    fi
}

# Function to run a specific test with proper output management
run_test() {
    local test_name="$1"
    local test_file="$TESTS_DIR/${test_name}-test.js"
    local session_dir="$RESULTS_DIR/$(date +%Y%m%d_%H%M%S)"
    local result_file="$session_dir/${test_name}-test.json"
    local log_file="$session_dir/${test_name}-test.log"
    
    mkdir -p "$session_dir"
    
    if [[ ! -f "$test_file" ]]; then
        print_error "Test file not found: $test_file"
        return 1
    fi
    
    print_status "Running $test_name test with authentication..."
    echo -e "${CYAN}Test file: $test_file${NC}"
    echo -e "${CYAN}Results: $result_file${NC}"
    echo -e "${CYAN}Log: $log_file${NC}"
    echo ""
    
    # Show authentication status
    if [[ "$AUTH_ENABLED" == "true" ]]; then
        echo -e "${GREEN}🔐 Authentication: ENABLED${NC}"
        echo -e "${CYAN}   Identity Server: ${IDENTITY_SERVER_URL}${NC}"
        echo -e "${CYAN}   Client ID: ${CLIENT_ID}${NC}"
    else
        echo -e "${YELLOW}⚠️  Authentication: DISABLED${NC}"
    fi
    echo ""
    
    # Run K6 test with proper environment and output capture
    local start_time=$(date)
    if k6 run --out json="$result_file" "$test_file" 2>&1 | tee "$log_file"; then
        local end_time=$(date)
        print_status "$test_name test completed successfully!"
        echo -e "${GREEN}Started: $start_time${NC}"
        echo -e "${GREEN}Completed: $end_time${NC}"
        echo -e "${GREEN}Results saved to: $result_file${NC}"
        echo -e "${GREEN}Log saved to: $log_file${NC}"
        
        # Create symlink to latest results
        ln -sfn "$session_dir" "$RESULTS_DIR/latest"
        
        # Generate summary
        generate_test_summary "$test_name" "$result_file" "$session_dir"
        
        # Show tail of results for quick review
        echo ""
        echo -e "${PURPLE}=== Test Summary (last 10 lines) ===${NC}"
        tail -10 "$log_file"
        
    else
        print_error "$test_name test failed!"
        echo -e "${RED}Check log file: $log_file${NC}"
        return 1
    fi
}

# Function to generate test summary
generate_test_summary() {
    local test_name="$1"
    local result_file="$2"
    local session_dir="$3"
    
    if [[ -f "$result_file" ]]; then
        echo ""
        echo -e "${PURPLE}=== $test_name Test Summary ===${NC}"
        
        # Extract key metrics from JSON (basic parsing)
        local total_requests=$(grep -c '"type":"Point"' "$result_file" 2>/dev/null || echo "N/A")
        echo "Total Requests: $total_requests"
        
        # Create human-readable summary file
        local summary_file="$session_dir/${test_name}-summary.txt"
        echo "Test: $test_name" > "$summary_file"
        echo "Date: $(date)" >> "$summary_file"
        echo "Total Requests: $total_requests" >> "$summary_file"
        echo "Results File: $result_file" >> "$summary_file"
        echo "Log File: $session_dir/${test_name}-test.log" >> "$summary_file"
        echo "Authentication: ${AUTH_ENABLED:-false}" >> "$summary_file"
        
        if [[ "$AUTH_ENABLED" == "true" ]]; then
            echo "Identity Server: ${IDENTITY_SERVER_URL}" >> "$summary_file"
            echo "Client ID: ${CLIENT_ID}" >> "$summary_file"
        fi
        
        echo -e "${GREEN}Summary saved to: $summary_file${NC}"
        echo ""
    fi
}

# Function to show interactive menu
show_menu() {
    echo ""
    echo -e "${CYAN}Available Load Tests:${NC}"
    echo ""
    echo "  1) Smoke Test (30 seconds) - Quick API availability check"
    echo "  2) Baseline Test (5 minutes) - Normal user activity simulation"
    echo "  3) Stress Test (7 minutes) - Find system breaking points"
    echo "  4) Spike Test (4.5 minutes) - Sudden load increase testing"
    echo "  5) Endurance Test (15 minutes) - Long-term stability"
    echo "  6) Projects Test (2 minutes) - Optimization validation"
    echo ""
    echo -e "${PURPLE}Test Suites:${NC}"
    echo "  7) Quick Suite (Smoke + Baseline) - ~6 minutes"
    echo "  8) Full Suite (All tests) - ~34 minutes"
    echo ""
    echo -e "${YELLOW}Utilities:${NC}"
    echo "  9) Check API Status"
    echo " 10) View Recent Results"
    echo " 11) Clean Old Results"
    echo " 12) Test Authentication"
    echo ""
    echo "  0) Exit"
    echo ""
}

# Function to test authentication
test_authentication() {
    print_status "Testing authentication configuration..."
    
    if [[ "$AUTH_ENABLED" != "true" ]]; then
        print_warning "Authentication is disabled (AUTH_ENABLED != 'true')"
        return 1
    fi
    
    local token_endpoint="${IDENTITY_SERVER_URL}/connect/token"
    print_status "Testing token endpoint: $token_endpoint"
    
    local response=$(curl -k -s -X POST "$token_endpoint" \
        -H "Content-Type: application/x-www-form-urlencoded" \
        -d "grant_type=client_credentials&client_id=${CLIENT_ID}&client_secret=${CLIENT_SECRET}&scope=${SCOPE}")
    
    if echo "$response" | grep -q "access_token"; then
        print_status "✅ Authentication test successful!"
        echo -e "${GREEN}Token endpoint is working correctly${NC}"
        return 0
    else
        print_error "❌ Authentication test failed!"
        echo -e "${RED}Response: $response${NC}"
        return 1
    fi
}

# Function to run test suite
run_test_suite() {
    local suite_type="$1"
    
    case $suite_type in
        "quick")
            print_status "Running Quick Test Suite (Smoke + Baseline)..."
            run_test "smoke" && run_test "baseline"
            ;;
        "full")
            print_status "Running Full Test Suite (All tests)..."
            run_test "smoke" && \
            run_test "baseline" && \
            run_test "stress" && \
            run_test "spike" && \
            run_test "endurance" && \
            run_test "projects-focused"
            ;;
        *)
            print_error "Unknown test suite: $suite_type"
            return 1
            ;;
    esac
}

# Function to view recent results
view_recent_results() {
    print_status "Recent test results:"
    echo ""
    
    if [[ -d "$RESULTS_DIR" ]] && [[ $(ls -A "$RESULTS_DIR" 2>/dev/null) ]]; then
        ls -lt "$RESULTS_DIR"/*/*.json 2>/dev/null | head -10 | while read -r line; do
            echo "  $line"
        done
        echo ""
        if [[ -L "$RESULTS_DIR/latest" ]]; then
            echo -e "${GREEN}Latest results: $(readlink $RESULTS_DIR/latest)${NC}"
        fi
    else
        print_warning "No test results found in $RESULTS_DIR"
    fi
    echo ""
}

# Function to clean old results
clean_old_results() {
    print_status "Cleaning results older than 7 days..."
    
    if [[ -d "$RESULTS_DIR" ]]; then
        find "$RESULTS_DIR" -name "20*" -type d -mtime +7 -exec rm -rf {} \; 2>/dev/null || true
        print_status "Old results cleaned"
    else
        print_warning "Results directory not found"
    fi
}

# Main execution logic
main() {
    print_header
    
    # Check prerequisites
    check_k6_installation
    load_env_vars
    
    # Handle direct command execution
    if [[ $# -gt 0 ]]; then
        case "$1" in
            "smoke"|"baseline"|"stress"|"spike"|"endurance"|"projects")
                check_api_availability
                run_test "$1"
                ;;
            "projects-focused")
                check_api_availability
                run_test "projects-focused"
                ;;
            "quick")
                check_api_availability
                run_test_suite "quick"
                ;;
            "full")
                check_api_availability
                run_test_suite "full"
                ;;
            "status")
                check_api_availability
                ;;
            "results")
                view_recent_results
                ;;
            "clean")
                clean_old_results
                ;;
            "auth")
                test_authentication
                ;;
            "help"|-h|"--help")
                echo "Usage: $0 [test_name|command]"
                echo ""
                echo "Tests: smoke, baseline, stress, spike, endurance, projects-focused"
                echo "Suites: quick, full"
                echo "Commands: status, results, clean, auth, help"
                echo ""
                echo "Examples:"
                echo "  $0 smoke          # Run smoke test"
                echo "  $0 quick          # Run quick test suite"
                echo "  $0 auth           # Test authentication"
                echo "  $0 status         # Check API status"
                ;;
            *)
                print_error "Unknown command: $1"
                echo "Use '$0 help' for usage information"
                exit 1
                ;;
        esac
        return
    fi
    
    # Interactive mode
    check_api_availability
    
    while true; do
        show_menu
        read -p "Select option (0-12): " choice
        
        case $choice in
            1)
                run_test "smoke"
                ;;
            2)
                run_test "baseline"
                ;;
            3)
                run_test "stress"
                ;;
            4)
                run_test "spike"
                ;;
            5)
                run_test "endurance"
                ;;
            6)
                run_test "projects-focused"
                ;;
            7)
                run_test_suite "quick"
                ;;
            8)
                run_test_suite "full"
                ;;
            9)
                check_api_availability
                ;;
            10)
                view_recent_results
                ;;
            11)
                clean_old_results
                ;;
            12)
                test_authentication
                ;;
            0)
                print_status "Goodbye!"
                break
                ;;
            *)
                print_warning "Invalid option. Please select 0-12."
                ;;
        esac
        
        if [[ $choice != "9" && $choice != "10" && $choice != "11" && $choice != "12" ]]; then
            echo ""
            read -p "Press Enter to continue..." -r
        fi
    done
}

# Execute main function with all arguments
main "$@"