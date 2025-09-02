# E2E Testing Resource Management Guide

## Overview

This guide addresses resource constraints and out-of-memory issues that can occur during E2E testing with Playwright and Docker containers.

## Problem Analysis

The original issues were:

1. **SQL Server Memory Exhaustion**: Error 701 - "There is insufficient system memory in resource pool 'internal'"
2. **No Container Resource Limits**: Containers could consume unlimited system resources
3. **Shared Database**: E2E tests competed with development environment for database resources
4. **Memory Leaks**: Playwright browser instances not properly cleaned up

## Solutions Implemented

### 1. Container Resource Limits

All Docker containers now have resource limits:

- **Database Container**: 3GB memory limit, 2GB reserved
- **Backend Container**: 4GB memory limit, 2GB reserved  
- **Frontend Container**: 2GB memory limit, 1GB reserved
- **Nginx Container**: 256MB memory limit

### 2. Test-Specific Environment

Created `docker-compose.e2e.yml` with:
- Separate test database on different port (1434)
- Optimized SQL Server configuration for testing
- Reduced resource allocation for test workloads

### 3. SQL Server Optimization

SQL Server container configured with:
- `MSSQL_MEMORY_LIMIT_MB=1024` for test environment
- Optimized checkpoint and recovery settings
- Snapshot isolation enabled for better concurrency

### 4. Playwright Resource Management

Enhanced E2E test base class with:
- Chrome browser args to reduce memory usage
- Shorter timeouts to prevent hanging tests
- Proper cleanup with garbage collection
- Headless mode enforced for efficiency

## Usage

### Running E2E Tests

**Option 1: Use the comprehensive script (recommended, runs from anywhere):**

```bash
# From project root
./scripts/run-e2e-tests.sh

# Or from any directory
/path/to/RqmtMgmt/scripts/run-e2e-tests.sh
```

**Option 2: Use the local script (from docker-compose directory):**

```bash
# From docker-compose directory
cd docker-compose/
./run-e2e-tests-local.sh
```

Both scripts:
1. Clean up existing containers
2. Check available system resources
3. Start test-specific containers with appropriate resource limits
4. Run database migrations
5. Execute E2E tests
6. Clean up resources

### Resource Monitoring

Monitor resource usage during tests:

```bash
./scripts/monitor-resources.sh
```

### Manual Setup

For manual testing setup:

```bash
# From docker-compose directory
cd docker-compose/

# Start test environment (choose based on available memory)
docker-compose -f docker-compose.e2e.yml up -d                                    # Normal memory
# OR
docker-compose -f docker-compose.e2e.yml -f docker-compose.e2e.low-memory.yml up -d  # Low memory

# Run database migrations
docker-compose -f docker-compose.e2e.yml exec backend dotnet ef database update

# Run tests from frontend.E2ETests directory
cd ../frontend.E2ETests/
dotnet test --parallel -- MSTest.Parallel.Workers=2

# Cleanup
cd ../docker-compose/
docker-compose -f docker-compose.e2e.yml down --volumes
```

## System Requirements

### Minimum Requirements
- **RAM**: 8GB total system memory
- **Available RAM**: 4GB free before starting tests
- **CPU**: 4 cores recommended
- **Disk**: 10GB free space for containers and logs

### Optimal Configuration
- **RAM**: 16GB total system memory
- **Available RAM**: 8GB free before starting tests
- **CPU**: 8 cores
- **SSD**: For improved I/O performance

## Configuration Files

### Resource Limits
- `docker-compose.yml`: Production environment with resource limits
- `docker-compose.e2e.yml`: Test environment configuration
- `docker-compose.e2e.low-memory.yml`: Reduced limits for constrained systems

### Application Settings
- `appsettings.E2ETesting.json`: E2E test-specific configuration
- `xunit.runner.json`: Test parallelization settings
- `optimize-test-database.sql`: SQL Server optimization script

## Troubleshooting

### Out of Memory Issues

1. **Check available memory**:
   ```bash
   free -h
   ```

2. **Monitor container usage**:
   ```bash
   docker stats
   ```

3. **Use low-memory configuration**:
   ```bash
   docker-compose -f docker-compose.e2e.yml -f docker-compose.e2e.low-memory.yml up -d
   ```

### Test Failures

1. **Increase timeouts** in test configuration
2. **Reduce parallelization**:
   ```bash
   dotnet test -- MSTest.Parallel.Workers=1
   ```
3. **Check container logs**:
   ```bash
   docker-compose -f docker-compose.e2e.yml logs
   ```

### Database Connection Issues

1. **Verify database is healthy**:
   ```bash
   docker-compose -f docker-compose.e2e.yml ps
   ```

2. **Check database logs**:
   ```bash
   docker-compose -f docker-compose.e2e.yml logs db-test
   ```

3. **Manually test connection**:
   ```bash
   sqlcmd -S localhost,1434 -U sa -P Your_password123 -Q "SELECT @@VERSION"
   ```

## Best Practices

1. **Always use the test script** for consistent environment setup
2. **Monitor resource usage** during test development
3. **Clean up containers** between test runs
4. **Use headless browser mode** for efficiency
5. **Limit test parallelization** on resource-constrained systems
6. **Separate test and development databases** to avoid conflicts

## Performance Tuning

### For Limited Memory Systems
- Use `docker-compose.e2e.low-memory.yml` override
- Reduce browser instances: `MSTest.Parallel.Workers=1`
- Increase swap space if available

### For High-Performance Systems
- Increase parallel workers: `MSTest.Parallel.Workers=4`
- Use larger resource allocations
- Enable browser caching for faster page loads

## Monitoring and Alerts

The resource monitoring script logs usage to `resource-usage.log`. Set up alerts for:
- Memory usage > 90% of container limits
- System available memory < 1GB
- Test execution time > 30 minutes

## Related Files

- `scripts/run-e2e-tests.sh`: Main test execution script
- `scripts/monitor-resources.sh`: Resource monitoring utility
- `scripts/optimize-test-database.sql`: Database optimization
- `frontend.E2ETests/E2ETestBase.cs`: Enhanced test base class
- Docker Compose configurations for different environments
