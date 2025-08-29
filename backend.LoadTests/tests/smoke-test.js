import { check } from 'k6';
import { config, getApiUrl, getThresholds } from '../config/test-config.js';
import { 
  makeApiRequest, 
  logTestStart, 
  logPerformanceExpectations,
  createStandardApiChecks 
} from '../utils/test-helpers.js';
import { validateAuthentication, logAuthConfig } from '../utils/auth-helpers.js';

// Test configuration
export let options = {
  vus: config.loadPatterns.smoke.vus,
  duration: config.loadPatterns.smoke.duration,
  thresholds: getThresholds(),
  
  // Disable some features for quick smoke test
  noConnectionReuse: false,
  userAgent: 'K6-SmokeTest/1.0',
  
  // Summary configuration
  summaryTrendStats: ['avg', 'min', 'med', 'max', 'p(95)', 'p(99)'],
  summaryTimeUnit: 'ms'
};

export function setup() {
  logTestStart('Smoke Test', 'Quick API availability and authentication validation');
  logAuthConfig();
  
  // Validate authentication before starting tests
  if (!validateAuthentication()) {
    throw new Error('Authentication validation failed. Please ensure IdentityServer is running and configured correctly.');
  }
  
  logPerformanceExpectations({
    'Duration': '30 seconds',
    'Virtual Users': '2',
    'Target Response Time': '< 100ms (95th percentile)',
    'Error Rate': '< 1%',
    'Primary Focus': 'API availability, authentication, and basic connectivity'
  });
  
  console.log('🔍 Testing endpoints:');
  console.log(`   Health: ${getApiUrl(config.endpoints.health)} (no auth)`);
  console.log(`   Projects: ${getApiUrl(config.endpoints.projectsWithPaging)} (with auth)`);
  console.log('');
}

export default function() {
  // Test 1: Health Check - Should be fastest and no auth required
  const healthResult = makeApiRequest(
    getApiUrl(config.endpoints.health),
    'health_check',
    {
      'response time < 50ms': (r) => r.timings.duration < 50,
      'status is healthy': (r) => r.status === 200
    },
    false // Explicitly no auth required
  );
  
  // Test 2: Projects API - Our optimized endpoint with authentication
  const projectsResult = makeApiRequest(
    getApiUrl(config.endpoints.projectsWithPaging),
    'projects_api',
    {
      'response time < 100ms': (r) => r.timings.duration < 100,
      'response time < 50ms': (r) => r.timings.duration < 50,
      'response time < 20ms': (r) => r.timings.duration < 20, // Post-optimization target
      'has projects data': (r) => {
        try {
          const data = JSON.parse(r.body);
          return data && (data.items || Array.isArray(data) || data.id);
        } catch {
          return false;
        }
      },
      'authenticated request': (r) => r.status !== 401 && r.status !== 403
    },
    true // Explicitly require auth
  );
  
  // Log results with authentication context
  if (!healthResult.success) {
    console.error('❌ Health check failed - API may be unavailable');
  } else {
    console.log(`✅ Health check passed: ${healthResult.duration}ms`);
  }
  
  if (!projectsResult.success) {
    if (projectsResult.response && projectsResult.response.status === 401) {
      console.error('❌ Projects API failed - Authentication issue (401 Unauthorized)');
      console.error('   Check IdentityServer configuration and client credentials');
    } else if (projectsResult.response && projectsResult.response.status === 403) {
      console.error('❌ Projects API failed - Authorization issue (403 Forbidden)');
      console.error('   Check API scopes and client permissions');
    } else {
      console.error('❌ Projects API failed - Performance optimization may have issues');
    }
  } else if (projectsResult.duration > 50) {
    console.warn(`⚠️  Projects API response time: ${projectsResult.duration}ms (expected < 20ms after optimization)`);
  } else {
    console.log(`✅ Projects API performing well: ${projectsResult.duration}ms`);
  }
}

export function teardown(data) {
  console.log('');
  console.log('🏁 Smoke Test Complete');
  console.log('');
  console.log('💡 Next Steps:');
  console.log('   - If smoke test passes: Run baseline test (npm run baseline)');
  console.log('   - If Projects API > 50ms: Investigate performance regression');
  console.log('   - If health check fails: Verify containers are running');
  console.log('   - If authentication fails: Check IdentityServer and client configuration');
  console.log('');
}