# Quick Start Guide

## Installation

1. **Install K6:**
   ```bash
   # Ubuntu/Debian
   sudo apt-get install k6
   
   # macOS
   brew install k6
   
   # Windows
   winget install k6
   ```

2. **Verify Installation:**
   ```bash
   k6 version
   ```

## Running Tests

### Interactive Mode
```bash
./run-tests.sh
```

### Direct Commands
```bash
# Quick tests
./run-tests.sh smoke          # 30 seconds
./run-tests.sh baseline       # 5 minutes
./run-tests.sh projects       # 2 minutes (optimization test)

# Comprehensive tests
./run-tests.sh stress         # 7 minutes
./run-tests.sh spike          # 4.5 minutes
./run-tests.sh endurance      # 15 minutes

# Test suites
./run-tests.sh quick          # Smoke + Baseline (~6 min)
./run-tests.sh full           # All tests (~34 min)
```

### NPM Scripts (Alternative)
```bash
npm run smoke
npm run baseline
npm run projects
```

## Expected Results

### Projects API (Post-Optimization)
- **Average Response Time**: 11-15ms
- **95th Percentile**: < 30ms
- **99th Percentile**: < 50ms
- **Error Rate**: < 0.5%

### Other APIs
- **Average Response Time**: < 100ms
- **95th Percentile**: < 200ms
- **Error Rate**: < 1%

## Troubleshooting

### SSL Issues
```bash
# For development with self-signed certificates
export K6_INSECURE_SKIP_TLS_VERIFY=true
```

### API Unavailable
```bash
# Check containers
docker-compose -f docker-compose.identity.yml up -d

# Test API directly
curl -k https://rqmtmgmt.local/health
```

### Performance Issues
- Check if Projects API response times > 50ms (potential regression)
- Verify containers have sufficient resources
- Review application logs for errors