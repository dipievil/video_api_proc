#!/bin/bash

# Script para executar testes de integração da Video Processing API
# Usage: ./run-integration-tests.sh [test-pattern]

set -e

echo "🎬 Video Processing API - Integration Tests"
echo "============================================="

# Navigate to project root
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(dirname "$SCRIPT_DIR")"
cd "$PROJECT_ROOT"

# Check if Docker and Docker Compose are installed
if ! command -v docker &> /dev/null; then
    echo "❌ Docker is not installed. Please install Docker first."
    exit 1
fi

# Check for Docker Compose (v2 or v1)
DOCKER_COMPOSE_CMD=""
if docker compose version &> /dev/null; then
    DOCKER_COMPOSE_CMD="docker compose"
    echo "✅ Docker Compose v2 found"
elif command -v docker-compose &> /dev/null; then
    DOCKER_COMPOSE_CMD="docker-compose"
    echo "✅ Docker Compose v1 found"
else
    echo "❌ Docker Compose is not installed. Please install Docker Compose."
    exit 1
fi

# Check if .NET 8 is installed
if ! dotnet --version | grep -q "8\."; then
    echo "❌ .NET 8 is required. Please install .NET 8 SDK."
    exit 1
fi

echo "✅ Prerequisites check passed"

# Clean up any existing test containers
echo "🧹 Cleaning up existing test environment..."
$DOCKER_COMPOSE_CMD -f docker-compose.test.yml down --volumes --remove-orphans 2>/dev/null || true

# Remove test directories if they exist
rm -rf tests/uploads tests/processed tests/db tests/logs 2>/dev/null || true

echo "🔧 Building test project..."
dotnet build tests/VideoProcessingApi.IntegrationTests/VideoProcessingApi.IntegrationTests.csproj

# Run tests with specified pattern or all tests
TEST_PATTERN="${1:-}"
if [ -n "$TEST_PATTERN" ]; then
    echo "🧪 Running integration tests matching pattern: $TEST_PATTERN"
    dotnet test tests/VideoProcessingApi.IntegrationTests/VideoProcessingApi.IntegrationTests.csproj \
        --logger "console;verbosity=normal" \
        --filter "FullyQualifiedName~$TEST_PATTERN"
else
    echo "🧪 Running all integration tests..."
    dotnet test tests/VideoProcessingApi.IntegrationTests/VideoProcessingApi.IntegrationTests.csproj \
        --logger "console;verbosity=normal"
fi

TEST_RESULT=$?

# Clean up test environment
echo "🧹 Cleaning up test environment..."
$DOCKER_COMPOSE_CMD -f docker-compose.test.yml down --volumes --remove-orphans 2>/dev/null || true
rm -rf tests/uploads tests/processed tests/db tests/logs 2>/dev/null || true

if [ $TEST_RESULT -eq 0 ]; then
    echo "✅ All integration tests passed!"
else
    echo "❌ Some integration tests failed. Exit code: $TEST_RESULT"
fi

exit $TEST_RESULT