#!/bin/bash

# E2E Test Helper - Choose your test runner
echo "🧪 RqmtMgmt E2E Test Runner Helper"
echo ""
echo "Choose how to run E2E tests:"
echo ""
echo "1) Full test runner (recommended) - works from anywhere"
echo "2) Local test runner - works from docker-compose directory"
echo "3) Manual setup instructions"
echo ""
read -p "Enter your choice (1-3): " choice

case $choice in
    1)
        echo ""
        echo "🚀 Running full test runner..."
        SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
        PROJECT_ROOT="$(dirname "$SCRIPT_DIR")"
        exec "$PROJECT_ROOT/scripts/run-e2e-tests.sh"
        ;;
    2)
        echo ""
        echo "🏃 Running local test runner..."
        SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
        PROJECT_ROOT="$(dirname "$SCRIPT_DIR")"
        cd "$PROJECT_ROOT/docker-compose"
        exec "./run-e2e-tests-local.sh"
        ;;
    3)
        echo ""
        echo "📖 Manual Setup Instructions:"
        echo ""
        echo "1. Navigate to docker-compose directory:"
        echo "   cd docker-compose/"
        echo ""
        echo "2. Start test containers:"
        echo "   docker-compose -f docker-compose.e2e.yml up -d"
        echo ""
        echo "3. Run database migrations:"
        echo "   docker-compose -f docker-compose.e2e.yml exec backend dotnet ef database update"
        echo ""
        echo "4. Run tests:"
        echo "   cd ../frontend.E2ETests/"
        echo "   dotnet test --parallel -- MSTest.Parallel.Workers=2"
        echo ""
        echo "5. Cleanup:"
        echo "   cd ../docker-compose/"
        echo "   docker-compose -f docker-compose.e2e.yml down --volumes"
        echo ""
        ;;
    *)
        echo ""
        echo "❌ Invalid choice. Please run the script again and choose 1, 2, or 3."
        exit 1
        ;;
esac
