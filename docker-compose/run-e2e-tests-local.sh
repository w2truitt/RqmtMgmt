#!/bin/bash

# Simple E2E Test Runner (run from docker-compose directory)
# This is a simplified version that assumes you're in the docker-compose directory

set -e

echo "🚀 Starting E2E Test Environment (simplified)..."

# Check if we're in the right directory
if [ ! -f "docker-compose.e2e.yml" ]; then
    echo "❌ Error: docker-compose.e2e.yml not found in current directory"
    echo "Please run this script from the docker-compose/ directory, or use ../scripts/run-e2e-tests.sh from anywhere"
    exit 1
fi

# Stop any existing containers to free up resources
echo "🧹 Cleaning up existing containers..."
docker-compose -f docker-compose.e2e.yml down --volumes --remove-orphans 2>/dev/null || true

# Check available system resources
echo "📊 Checking system resources..."
AVAILABLE_MEM=$(free -m | awk 'NR==2{printf "%.0f", $7}')
echo "Available Memory: ${AVAILABLE_MEM}MB"

# Set compose files based on available memory
if [ "$AVAILABLE_MEM" -lt 6144 ]; then
    echo "📉 Using reduced memory configuration"
    COMPOSE_FILES="-f docker-compose.e2e.yml -f docker-compose.e2e.low-memory.yml"
else
    COMPOSE_FILES="-f docker-compose.e2e.yml"
fi

# Start the test environment
echo "🏗️  Building and starting test containers..."
docker-compose $COMPOSE_FILES up -d --build

# Wait for services to be healthy
echo "⏳ Waiting for services to be ready..."
max_attempts=30
attempt=0

while [ $attempt -lt $max_attempts ]; do
    if docker-compose $COMPOSE_FILES ps --quiet backend | xargs docker inspect --format='{{.State.Health.Status}}' 2>/dev/null | grep -q "healthy"; then
        echo "✅ Backend service is healthy"
        break
    fi
    echo "⏳ Waiting for backend service... (attempt $((attempt + 1))/$max_attempts)"
    sleep 5
    attempt=$((attempt + 1))
done

if [ $attempt -eq $max_attempts ]; then
    echo "❌ Services failed to start within timeout"
    docker-compose $COMPOSE_FILES logs
    exit 1
fi

# Run database migrations
echo "🗄️  Setting up test database..."
docker-compose $COMPOSE_FILES exec -T backend dotnet ef database update || {
    echo "❌ Database migration failed"
    docker-compose $COMPOSE_FILES logs backend
    exit 1
}

# Run the E2E tests
echo "🧪 Running E2E tests..."
cd ../frontend.E2ETests

export ASPNETCORE_ENVIRONMENT=Testing
export TEST_BASE_URL=http://localhost:8080

dotnet test \
    --configuration Release \
    --logger "console;verbosity=normal" \
    --parallel \
    -- \
    MSTest.Parallel.Workers=2

TEST_EXIT_CODE=$?

# Cleanup
echo "🧹 Cleaning up test environment..."
cd ../docker-compose
docker-compose $COMPOSE_FILES down --volumes

if [ $TEST_EXIT_CODE -eq 0 ]; then
    echo "✅ E2E tests completed successfully!"
else
    echo "❌ E2E tests failed with exit code $TEST_EXIT_CODE"
fi

exit $TEST_EXIT_CODE
