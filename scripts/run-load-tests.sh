#!/bin/bash

# NBomber Load Testing Runner Script
# Provides easy access to different load testing scenarios

PROJECT_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
LOAD_TESTS_DIR="$PROJECT_ROOT/backend.LoadTests"

echo "🚀 Requirements Management API Load Testing"
echo "=========================================="
echo "Project Root: $PROJECT_ROOT"
echo "Load Tests Dir: $LOAD_TESTS_DIR"
echo ""

# Check if load tests project exists
if [ ! -d "$LOAD_TESTS_DIR" ]; then
    echo "❌ Load tests project not found at: $LOAD_TESTS_DIR"
    exit 1
fi

# Function to run a specific test type
run_load_test() {
    local test_type=$1
    local description=$2
    
    echo "🎯 Running $description..."
    echo "Test Type: $test_type"
    echo "Time: $(date)"
    echo "----------------------------------------"
    
    cd "$LOAD_TESTS_DIR"
    dotnet run -- "$test_type"
    
    echo ""
    echo "✅ $description completed!"
    echo "📊 Check the 'reports' folder for detailed results"
    echo ""
}

# Main menu
if [ $# -eq 0 ]; then
    echo "Available load test types:"
    echo ""
    echo "1) baseline   - Normal user activity simulation (5-10 req/sec, 2 min)"
    echo "2) stress     - Find breaking point (10-100 req/sec, 4 min)"
    echo "3) spike      - Sudden load increase test (5-100-5 req/sec)"
    echo "4) endurance  - Long-running stability test (10 req/sec, 10 min)"
    echo "5) projects   - Projects API specific test (20 req/sec, 2 min)"
    echo ""
    echo "Usage: $0 [test_type]"
    echo "Example: $0 baseline"
    echo ""
    
    read -p "Enter test type (or press Enter for baseline): " choice
    choice=${choice:-baseline}
else
    choice=$1
fi

# Run the selected test
case $choice in
    "1"|"baseline")
        run_load_test "baseline" "Baseline Load Test"
        ;;
    "2"|"stress")
        run_load_test "stress" "Stress Test"
        ;;
    "3"|"spike")
        run_load_test "spike" "Spike Test"
        ;;
    "4"|"endurance")
        run_load_test "endurance" "Endurance Test"
        ;;
    "5"|"projects")
        run_load_test "projects" "Projects API Specific Test"
        ;;
    *)
        echo "❌ Unknown test type: $choice"
        echo "Available types: baseline, stress, spike, endurance, projects"
        exit 1
        ;;
esac

echo "🎉 Load testing session completed!"
echo "📈 Reports available in: $LOAD_TESTS_DIR/reports/"
echo ""