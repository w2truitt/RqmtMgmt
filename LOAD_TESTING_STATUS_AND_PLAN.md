# Load Testing Status and Recovery Plan

**Date**: August 29, 2025  
**Status**: 🔄 **AUTHENTICATION ISSUES IDENTIFIED - RECOVERY PLAN ACTIVE**  
**Priority**: HIGH - Breaking the rework loop and implementing sustainable solution

---

## 🚨 **Current Situation Analysis**

Based on the API_PERFORMANCE_OPTIMIZATION_STATUS.md and your feedback, we are in a **rework loop** that needs to be broken:

### **What's Working**:
- ✅ K6 load testing framework is implemented and functional
- ✅ API performance optimizations completed (99.95% improvement)
- ✅ Test infrastructure is comprehensive (6 test types)

### **What's Broken**:
- ❌ K6 tests cannot authenticate with the backend API
- ❌ Authentication was working with IdentityServer, then disabled for "load testing purposes"
- ❌ Currently using `docker-compose.loadtests.yml` that removes IdentityServer
- ❌ Test output is being lost due to console output and token limits

### **Root Problem**:
**We abandoned a working authentication solution in favor of a non-working bypass approach.**

---

## 🎯 **Recovery Plan - Breaking the Rework Loop**

### **Decision Point**: Restore IdentityServer Authentication
**Rationale**: 
- Authentication was working before
- Load testing with auth is more realistic and valuable
- Removing auth created more problems than it solved
- Production systems will have auth, so tests should too

### **Phase 1: Immediate Recovery (1-2 hours)**

#### **1.1 Restore Working Docker Configuration**
- ✅ **Action**: Switch back to `docker-compose/docker-compose.identity.yml`
- ✅ **Reason**: This was the working configuration
- ✅ **Impact**: Restores IdentityServer integration

#### **1.2 Implement Proper Test Output Management**
- ✅ **Action**: Redirect all K6 test output to files instead of console
- ✅ **Reason**: Prevents token limit issues and enables investigation
- ✅ **Files**: Create `load-test-results/` directory for all outputs

#### **1.3 Restore K6 Authentication**
- ✅ **Action**: Re-implement the working K6 authentication code
- ✅ **Reason**: This was functional before being removed
- ✅ **Impact**: Tests can authenticate and access protected endpoints

---

## 📋 **Implementation Steps**

### **Step 1: Restore Docker Configuration**
```bash
# Stop current containers
cd docker-compose/
docker-compose -f docker-compose.loadtests.yml down

# Start with IdentityServer
docker-compose -f docker-compose.yml -f docker-compose.identity.yml up -d

# Verify services are running
docker-compose ps
```

### **Step 2: Create Test Output Management**
```bash
# Create results directory
mkdir -p load-test-results/$(date +%Y%m%d_%H%M%S)

# Update K6 scripts to output to files
# Redirect both stdout and stderr to timestamped files
```

### **Step 3: Restore K6 Authentication**
```javascript
// Re-implement the working authentication flow in K6
export function authenticate() {
    // Get token from IdentityServer
    const tokenResponse = http.post(`${IDENTITY_SERVER_URL}/connect/token`, {
        grant_type: 'client_credentials',
        client_id: 'loadtest_client',
        client_secret: 'loadtest_secret',
        scope: 'api'
    });
    
    check(tokenResponse, {
        'authentication successful': (r) => r.status === 200,
    });
    
    return JSON.parse(tokenResponse.body).access_token;
}
```

### **Step 4: Update Test Execution Scripts**
```bash
# Modify run-tests.sh to:
# 1. Create timestamped output directory
# 2. Redirect all output to files
# 3. Show tail of output for monitoring
# 4. Preserve full results for investigation
```

---

## 🔧 **Technical Implementation Details**

### **Authentication Configuration**

#### **IdentityServer Client Setup**:
```json
{
    "ClientId": "loadtest_client",
    "ClientName": "Load Test Client",
    "AllowedGrantTypes": ["client_credentials"],
    "ClientSecrets": [{"Value": "loadtest_secret"}],
    "AllowedScopes": ["api"],
    "AccessTokenLifetime": 3600
}
```

#### **K6 Authentication Module**:
```javascript
import { check } from 'k6';
import http from 'k6/http';

export class AuthService {
    constructor(baseUrl, identityUrl) {
        this.baseUrl = baseUrl;
        this.identityUrl = identityUrl;
        this.token = null;
    }
    
    authenticate() {
        if (this.token) return this.token;
        
        const response = http.post(`${this.identityUrl}/connect/token`, {
            grant_type: 'client_credentials',
            client_id: 'loadtest_client',
            client_secret: 'loadtest_secret',
            scope: 'api'
        }, {
            headers: { 'Content-Type': 'application/x-www-form-urlencoded' }
        });
        
        check(response, {
            'auth successful': (r) => r.status === 200,
        });
        
        this.token = JSON.parse(response.body).access_token;
        return this.token;
    }
    
    getHeaders() {
        return {
            'Authorization': `Bearer ${this.authenticate()}`,
            'Content-Type': 'application/json'
        };
    }
}
```

### **Output Management System**

#### **Directory Structure**:
```
load-test-results/
├── 20250829_143000/          # Timestamped session
│   ├── smoke-test.json       # Full K6 output
│   ├── baseline-test.json
│   ├── stress-test.json
│   ├── session-summary.md    # Human-readable summary
│   └── errors.log           # Error details
└── latest/                  # Symlink to most recent
```

#### **Test Execution Pattern**:
```bash
#!/bin/bash
# Create session directory
SESSION_DIR="load-test-results/$(date +%Y%m%d_%H%M%S)"
mkdir -p "$SESSION_DIR"
ln -sfn "$SESSION_DIR" load-test-results/latest

# Run test with output capture
echo "Running $TEST_NAME..."
k6 run --out json="$SESSION_DIR/$TEST_NAME.json" \
       tests/$TEST_NAME.js 2>&1 | tee "$SESSION_DIR/$TEST_NAME.log"

# Show summary (last 20 lines)
echo "=== Test Summary ==="
tail -20 "$SESSION_DIR/$TEST_NAME.log"

# Save to investigation file
echo "Full results saved to: $SESSION_DIR/$TEST_NAME.json"
```

---

## 📊 **Success Criteria**

### **Immediate Success (Phase 1)**:
- [ ] IdentityServer is running and accessible
- [ ] K6 tests can authenticate successfully
- [ ] Test results are saved to files for investigation
- [ ] At least smoke test passes with authentication

### **Short-term Success (1-2 days)**:
- [ ] All 6 K6 test types pass with authentication
- [ ] Comprehensive test results are available for analysis
- [ ] Performance baselines are established with auth overhead
- [ ] No more token limit issues during testing

### **Long-term Success (1 week)**:
- [ ] Automated test execution with result archiving
- [ ] Performance regression detection
- [ ] Clear documentation for ongoing maintenance
- [ ] Integration with CI/CD pipeline

---

## 🛡️ **Risk Mitigation**

### **Preventing Future Rework Loops**:
1. **Document Decisions**: Every change must be documented with rationale
2. **Preserve Working Solutions**: Never remove working code without backup
3. **Incremental Changes**: Test each change before proceeding
4. **Output Preservation**: Always save full test results for investigation

### **Authentication Backup Plan**:
If IdentityServer auth continues to fail:
1. **Option A**: Implement API key authentication for load testing
2. **Option B**: Create dedicated load testing endpoints with minimal auth
3. **Option C**: Use test user credentials with OAuth password flow

---

## 🔄 **Current Action Items**

### **Immediate (Today)**:
1. [ ] **Stop current load test containers**
2. [ ] **Start containers with IdentityServer** (`docker-compose.identity.yml`)
3. [ ] **Create load-test-results directory structure**
4. [ ] **Test IdentityServer accessibility** (curl token endpoint)

### **Next Session**:
1. [ ] **Restore K6 authentication code**
2. [ ] **Update test scripts for file output**
3. [ ] **Run smoke test with authentication**
4. [ ] **Verify results are saved properly**

### **Following Session**:
1. [ ] **Run full test suite with authentication**
2. [ ] **Analyze performance impact of authentication**
3. [ ] **Document final authentication approach**
4. [ ] **Create maintenance procedures**

---

## 📝 **Decision Log**

### **Decision 1: Restore IdentityServer Authentication**
- **Date**: August 29, 2025
- **Rationale**: Authentication was working, removal created more problems
- **Impact**: More realistic load testing, matches production environment
- **Status**: Approved for implementation

### **Decision 2: File-Based Test Output**
- **Date**: August 29, 2025  
- **Rationale**: Console output causes token limits and prevents investigation
- **Impact**: Enables proper result analysis and troubleshooting
- **Status**: Approved for implementation

### **Decision 3: Break Rework Loop**
- **Date**: August 29, 2025
- **Rationale**: Current approach is creating circular problems
- **Impact**: Focus on working solutions, document decisions
- **Status**: Active implementation

---

**Next Update**: After Phase 1 implementation (IdentityServer restoration)  
**Owner**: HCHB Code Agent  
**Review Date**: August 30, 2025