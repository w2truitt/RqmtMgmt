# K6 Load Testing Suite

## Overview

Comprehensive load testing framework for the Requirements Management API using K6. This project validates that our recent performance optimizations (99.95% improvement in Projects API) hold up under various load conditions.

## Recent Performance Context

- **Projects API Optimization**: Reduced response time from 30+ seconds to 11-15ms
- **E2E Test Improvement**: From 5+ minute timeouts to 38-39 second completion
- **Root Cause Fixed**: Eliminated expensive EF Core projection subqueries

## Prerequisites

### Install K6

**Ubuntu/Debian:**
```bash
sudo gpg -k
sudo gpg --no-default-keyring --keyring /usr/share/keyrings/k6-archive-keyring.gpg --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys C5AD17C747E3415A3642D57D77C6C491D6AC1D69
echo "deb [signed-by=/usr/share/keyrings/k6-archive-keyring.gpg] https://dl.k6.io/deb stable main" | sudo tee /etc/apt/sources.list.d/k6.list
sudo apt-get update
sudo apt-get install k6
```

**macOS:**
```bash
brew install k6
```

**Windows:**
```bash
winget install k6
```

**Docker (Alternative):**
```bash
docker pull grafana/k6:latest
```

## Test Types

### 1. Smoke Test (30 seconds)
Quick API availability check to ensure basic functionality.
```bash
npm run smoke
# or directly:
k6 run tests/smoke-test.js
```

### 2. Baseline Test (5 minutes)
Simulates normal user activity across multiple API endpoints:
- Projects API: 10 requests/second
- Mixed APIs: 8 requests/second
- Search APIs: 5 requests/second
- Health checks: 1 request/second

```bash
npm run baseline
```

### 3. Stress Test (7 minutes)
Gradually increases load to find system breaking points:
- Starts at 5 requests/second
- Increases to 75 requests/second
- Identifies maximum sustainable load

```bash
npm run stress
```

### 4. Spike Test (4.5 minutes)
Tests system response to sudden load increases:
- Normal load: 5 requests/second
- Sudden spike: 50 requests/second for 30 seconds
- Return to normal: 5 requests/second

```bash
npm run spike
```

### 5. Endurance Test (15 minutes)
Long-term stability test with sustained moderate load:
- Projects API: 8 requests/second
- Mixed APIs: Continuous load
- Validates memory leaks and performance degradation

```bash
npm run endurance
```

### 6. Projects-Focused Test (2 minutes)
Intensive testing of the optimized Projects API:
- Projects API: 20 requests/second
- Validates our 99.95% performance improvement

```bash
npm run projects
```

## API Endpoints Tested

- **GET /api/Projects** - Recently optimized (primary focus)
- **GET /api/Users** - User management
- **GET /api/Requirements** - Requirements with search/filtering
- **GET /api/TestCases** - Test case management
- **GET /api/TestSuites** - Test suite management
- **GET /health** - System health monitoring

## Configuration

### Environment Variables

Create a `.env` file or set these environment variables:

```bash
# API Configuration
API_BASE_URL=https://rqmtmgmt.local
API_TIMEOUT=30s

# Performance Thresholds
MAX_RESPONSE_TIME=100
ERROR_RATE_THRESHOLD=0.01
MIN_THROUGHPUT=50

# Test Configuration
SMOKE_DURATION=30s
BASELINE_DURATION=5m
STRESS_DURATION=7m
SPIKE_DURATION=4m30s
ENDURANCE_DURATION=15m
```

### SSL Configuration

For self-signed certificates (development):
```bash
k6 run --insecure-skip-tls-verify tests/smoke-test.js
```

Or add to `/etc/hosts`:
```
127.0.0.1   rqmtmgmt.local
```

## Reports

K6 automatically generates detailed reports. For enhanced reporting:

### HTML Reports
```bash
k6 run --out json=results.json tests/baseline-test.js
# Convert to HTML using k6-reporter or similar tools
```

### InfluxDB + Grafana Integration
```bash
k6 run --out influxdb=http://localhost:8086/k6 tests/baseline-test.js
```

### CSV Export
```bash
k6 run --out csv=results.csv tests/baseline-test.js
```

## Performance Targets

Based on our recent optimizations:
- **Response Time**: < 100ms for 95th percentile (current: 11-15ms)
- **Throughput**: > 50 requests/second sustained
- **Error Rate**: < 1% under normal load
- **Memory**: Stable memory usage over time

## Usage Examples

```bash
# Quick health check
npm run smoke

# Comprehensive baseline testing
npm run baseline

# Find system limits
npm run stress

# Test resilience to traffic spikes
npm run spike

# Long-term stability validation
npm run endurance

# Focus on optimized Projects API
npm run projects

# Run all critical tests
npm run all
```

## Docker Usage

If using K6 via Docker:

```bash
# Smoke test
docker run --rm -v $(pwd):/app grafana/k6:latest run /app/tests/smoke-test.js

# Baseline test
docker run --rm -v $(pwd):/app grafana/k6:latest run /app/tests/baseline-test.js

# With custom options
docker run --rm -v $(pwd):/app grafana/k6:latest run --insecure-skip-tls-verify /app/tests/stress-test.js
```

## Integration with CI/CD

Add to build pipeline for automated performance regression testing:

```yaml
name: Load Testing
jobs:
  load-test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      
      - name: Install K6
        run: |
          sudo gpg --no-default-keyring --keyring /usr/share/keyrings/k6-archive-keyring.gpg --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys C5AD17C747E3415A3642D57D77C6C491D6AC1D69
          echo "deb [signed-by=/usr/share/keyrings/k6-archive-keyring.gpg] https://dl.k6.io/deb stable main" | sudo tee /etc/apt/sources.list.d/k6.list
          sudo apt-get update
          sudo apt-get install k6
      
      - name: Run Load Tests
        run: |
          cd backend.LoadTests
          npm run smoke
          npm run baseline
        
      - name: Upload Results
        uses: actions/upload-artifact@v3
        with:
          name: load-test-results
          path: backend.LoadTests/results/
```

## Troubleshooting

### Connection Issues
- Ensure containers are running: `docker-compose -f docker-compose.identity.yml up -d`
- Verify API accessibility: `curl https://rqmtmgmt.local/health`

### SSL Certificate Issues
- Use `--insecure-skip-tls-verify` flag for development
- Add `rqmtmgmt.local` to `/etc/hosts`: `127.0.0.1 rqmtmgmt.local`

### Performance Regression
- Compare current results with previous test runs
- Check for new expensive queries in logs
- Verify AsNoTracking optimizations are still active

### K6 Installation Issues
- Verify installation: `k6 version`
- Check system requirements: Node.js 16+ recommended
- Use Docker alternative if local installation fails

## Contributing

When adding new load test scenarios:
1. Create new test file in `tests/` directory
2. Follow existing naming convention: `{test-type}-test.js`
3. Add npm script to `package.json`
4. Update this README with test description
5. Document expected performance characteristics

## Advanced Features

### Custom Metrics
```javascript
import { Trend } from 'k6/metrics';

let customMetric = new Trend('custom_response_time');

export default function() {
  let response = http.get('https://rqmtmgmt.local/api/Projects');
  customMetric.add(response.timings.duration);
}
```

### Parameterized Tests
```bash
k6 run -e API_BASE_URL=https://staging.rqmtmgmt.local tests/baseline-test.js
```

### Load Profiles
```javascript
export let options = {
  stages: [
    { duration: '2m', target: 10 },   // Ramp up
    { duration: '5m', target: 50 },   // Stay at load
    { duration: '2m', target: 0 },    // Ramp down
  ],
  thresholds: {
    http_req_duration: ['p(95)<100'],
    http_req_failed: ['rate<0.01'],
  },
};
```