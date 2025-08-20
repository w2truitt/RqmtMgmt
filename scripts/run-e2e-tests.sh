#!/bin/bash

# E2E Test Runner with Resource Management
# This script sets up an isolated environment for E2E testing to prevent resource conflicts

set -e

echo "🚀 Starting E2E Test Environment..."

# Stop any existing containers to free up resources
echo "🧹 Cleaning up existing containers..."
docker-compose -f docker-compose.e2e.yml down --volumes --remove-orphans 2>/dev/null || true
docker system prune -f --volumes 2>/dev/null || true

# Check available system resources
echo "📊 Checking system resources..."
TOTAL_MEM=$(free -m | awk 'NR==2{printf "%.0f", $2}')
AVAILABLE_MEM=$(free -m | awk 'NR==2{printf "%.0f", $7}')
echo "Total Memory: ${TOTAL_MEM}MB, Available: ${AVAILABLE_MEM}MB"

if [ "$AVAILABLE_MEM" -lt 4096 ]; then
    echo "⚠️  Warning: Low available memory (${AVAILABLE_MEM}MB). E2E tests may fail."
    echo "   Consider closing other applications or increasing system memory."
fi

# Set memory limits based on available resources
if [ "$AVAILABLE_MEM" -lt 6144 ]; then
    echo "📉 Using reduced memory configuration for low-memory systems"
    export COMPOSE_FILE="docker-compose.e2e.yml:docker-compose.e2e.low-memory.yml"
fi

# Start the test environment
echo "🏗️  Building and starting test containers..."
docker-compose -f docker-compose.e2e.yml up -d --build

# Wait for services to be healthy
echo "⏳ Waiting for services to be ready..."
max_attempts=60
attempt=0

while [ $attempt -lt $max_attempts ]; do
    if docker-compose -f docker-compose.e2e.yml ps --quiet backend | xargs docker inspect --format='{{.State.Health.Status}}' | grep -q "healthy"; then
        echo "✅ Backend service is healthy"
        break
    fi
    echo "⏳ Waiting for backend service to be healthy... (attempt $((attempt + 1))/$max_attempts)"
    sleep 5
    attempt=$((attempt + 1))
done

if [ $attempt -eq $max_attempts ]; then
    echo "❌ Services failed to start within timeout"
    docker-compose -f docker-compose.e2e.yml logs
    exit 1
fi

# Run database migrations for test environment
echo "🗄️  Setting up test database..."
docker-compose -f docker-compose.e2e.yml exec -T backend dotnet ef database update || {
    echo "❌ Database migration failed"
    docker-compose -f docker-compose.e2e.yml logs backend
    exit 1
}

# Run the E2E tests
echo "🧪 Running E2E tests..."
cd ../frontend.E2ETests

# Set test-specific environment variables
export ASPNETCORE_ENVIRONMENT=Testing
export TEST_BASE_URL=http://localhost:8080

# Run tests with resource monitoring
dotnet test \
    --configuration Release \
    --logger "trx;LogFileName=TestResults.trx" \
    --logger "console;verbosity=normal" \
    --collect:"XPlat Code Coverage" \
    --settings ../coverlet.runsettings \
    --parallel \
    -- \
    MSTest.Parallel.Workers=2 \
    MSTest.Parallel.Scope=MethodLevel

TEST_EXIT_CODE=$?

# Cleanup
echo "🧹 Cleaning up test environment..."
cd ../docker-compose
docker-compose -f docker-compose.e2e.yml down --volumes

# Show resource usage summary
echo "📊 Resource usage summary:"
docker system df

if [ $TEST_EXIT_CODE -eq 0 ]; then
    echo "✅ E2E tests completed successfully!"
else
    echo "❌ E2E tests failed with exit code $TEST_EXIT_CODE"
fi

exit $TEST_EXIT_CODE
