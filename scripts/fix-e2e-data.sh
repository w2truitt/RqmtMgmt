#!/bin/bash

# Data Seeding and Verification Script for E2E Tests
# This script helps diagnose and fix missing test data issues

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"

echo "🔍 E2E Test Data Seeding and Verification Tool"
echo "=============================================="
echo ""

# Function to run specific test
run_test() {
    local test_name="$1"
    local description="$2"
    
    echo "📋 $description"
    echo "   Running: $test_name"
    echo ""
    
    cd "$PROJECT_ROOT"
    dotnet test frontend.E2ETests --filter "Name~$test_name" --logger "console;verbosity=detailed"
    echo ""
}

# Function to restart services
restart_services() {
    echo "🔄 Restarting Identity Server and Backend services..."
    echo ""
    
    kubectl delete pod -l app=identityserver --ignore-not-found=true
    kubectl delete pod -l app=backend --ignore-not-found=true
    
    echo "⏳ Waiting for services to restart..."
    sleep 10
    
    kubectl wait --for=condition=ready pod -l app=identityserver --timeout=60s
    kubectl wait --for=condition=ready pod -l app=backend --timeout=60s
    
    echo "✅ Services restarted successfully"
    echo ""
}

# Main execution
case "${1:-help}" in
    "verify-projects")
        run_test "DataSeeding_VerifyRequiredProjects_ReportsStatus" "Verifying required static projects exist"
        ;;
    
    "seed-projects")
        run_test "DataSeeding_SeedRequiredProjects_CreatesStaticProjects" "Seeding missing static projects"
        ;;
    
    "verify-users")
        run_test "DataSeeding_VerifyIdentityServerUsers_ChecksAuthentication" "Verifying Identity Server users can authenticate"
        ;;
    
    "restart-services")
        restart_services
        ;;
    
    "show-instructions")
        run_test "DataSeeding_ShowRecoveryInstructions_DisplaysActions" "Displaying data seeding recovery instructions"
        ;;
    
    "full-diagnosis")
        echo "🩺 Running full data diagnosis..."
        echo ""
        
        run_test "DataSeeding_VerifyRequiredProjects_ReportsStatus" "Step 1: Verifying projects"
        run_test "DataSeeding_VerifyIdentityServerUsers_ChecksAuthentication" "Step 2: Verifying users"
        run_test "DataSeeding_ShowRecoveryInstructions_DisplaysActions" "Step 3: Showing recovery instructions"
        ;;
    
    "fix-all")
        echo "🔧 Running complete data seeding fix..."
        echo ""
        
        echo "Step 1: Seeding missing projects..."
        run_test "DataSeeding_SeedRequiredProjects_CreatesStaticProjects" "Seeding projects"
        
        echo "Step 2: Restarting services to trigger user seeding..."
        restart_services
        
        echo "Step 3: Verifying everything is fixed..."
        run_test "DataSeeding_VerifyRequiredProjects_ReportsStatus" "Verifying projects"
        run_test "DataSeeding_VerifyIdentityServerUsers_ChecksAuthentication" "Verifying users"
        
        echo "✅ Data seeding fix completed!"
        ;;
    
    *)
        echo "Usage: $0 <command>"
        echo ""
        echo "Commands:"
        echo "  verify-projects    - Check if required static projects exist"
        echo "  seed-projects      - Create missing static projects"
        echo "  verify-users       - Check if Identity Server users can authenticate"
        echo "  restart-services   - Restart Identity Server and Backend pods"
        echo "  show-instructions  - Display detailed recovery instructions"
        echo "  full-diagnosis     - Run complete diagnosis (recommended first step)"
        echo "  fix-all           - Attempt to fix all missing data automatically"
        echo ""
        echo "Recommended workflow:"
        echo "1. $0 full-diagnosis    # Identify what's missing"
        echo "2. $0 fix-all          # Attempt to fix everything"
        echo "3. Run regular E2E tests to verify"
        echo ""
        ;;
esac