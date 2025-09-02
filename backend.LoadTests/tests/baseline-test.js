import { check, sleep } from 'k6';
import { config, getApiUrl, getThresholds } from '../config/test-config.js';
import { 
  makeApiRequest, 
  makeSequentialRequests,
  simulateThinkTime,
  logTestStart, 
  logPerformanceExpectations,
  createProjectsPerformanceChecks,
  createStandardApiChecks 
} from '../utils/test-helpers.js';
import { validateAuthentication, logAuthConfig } from '../utils/auth-helpers.js';

// Test configuration
export let options = {
  stages: config.loadPatterns.baseline.stages,
  thresholds: getThresholds(),
  
  // Realistic browser behavior
  noConnectionReuse: false,
  userAgent: 'K6-BaselineTest/1.0',
  
  // Enhanced reporting
  summaryTrendStats: ['avg', 'min', 'med', 'max', 'p(90)', 'p(95)', 'p(99)'],
  summaryTimeUnit: 'ms'
};

export function setup() {
  logTestStart('Baseline Load Test', 'Normal user activity simulation across multiple API endpoints');
  logAuthConfig();
  
  // Validate authentication before starting tests
  if (!validateAuthentication()) {
    throw new Error('Authentication validation failed. Please ensure IdentityServer is running and configured correctly.');
  }
  
  logPerformanceExpectations({
    'Duration': '5 minutes',
    'Load Pattern': 'Ramp 1m → Sustain 3m → Ramp down 1m',
    'Peak Virtual Users': '10',
    'Target Response Time': '< 100ms (95th percentile)',
    'Projects API Target': '< 20ms (post-optimization)',
    'Error Rate': '< 1%',
    'Min Throughput': '> 10 req/sec',
    'Focus': 'Sustained performance under normal load with authentication'
  });
  
  console.log('🔍 API Endpoints Under Test:');
  console.log(`   Projects (optimized): ${getApiUrl(config.endpoints.projectsWithPaging)} (auth)`);
  console.log(`   Users: ${getApiUrl(config.endpoints.users)} (auth)`);
  console.log(`   Requirements: ${getApiUrl(config.endpoints.requirements)} (auth)`);
  console.log(`   Health: ${getApiUrl(config.endpoints.health)} (no auth)`);
  console.log('');
  
  // Verify API availability before starting load test
  console.log('🔧 Pre-test API verification...');
  const healthCheck = makeApiRequest(getApiUrl(config.endpoints.health), 'pre_test_health', {}, false);
  if (!healthCheck.success) {
    throw new Error('❌ API health check failed - cannot proceed with baseline test');
  }
  console.log('✅ API is available, starting baseline test');
  console.log('');
}

export default function() {
  // Simulate realistic user session patterns
  const userBehavior = Math.random();
  
  if (userBehavior < 0.4) {
    // 40% - Project management workflow
    projectManagementWorkflow();
  } else if (userBehavior < 0.7) {
    // 30% - Requirements browsing workflow  
    requirementsBrowsingWorkflow();
  } else if (userBehavior < 0.9) {
    // 20% - Mixed API usage
    mixedApiWorkflow();
  } else {
    // 10% - Heavy Projects API usage (testing our optimization)
    projectsIntensiveWorkflow();
  }
}

function projectManagementWorkflow() {
  // Simulate user browsing projects, then viewing details
  const requests = [
    {
      url: getApiUrl(config.endpoints.projectsWithPaging),
      name: 'browse_projects',
      checks: createProjectsPerformanceChecks(),
      requireAuth: true
    },
    {
      url: getApiUrl(config.endpoints.projectById),
      name: 'view_project_details',
      checks: createProjectsPerformanceChecks(),
      requireAuth: true
    },
    {
      url: getApiUrl(config.endpoints.users),
      name: 'check_team_members',
      checks: createStandardApiChecks(),
      requireAuth: true
    }
  ];
  
  makeSequentialRequests(requests, 500); // 500ms between requests
  simulateThinkTime(2, 5); // User reads/thinks for 2-5 seconds
}

function requirementsBrowsingWorkflow() {
  // Simulate user working with requirements
  const requests = [
    {
      url: getApiUrl(config.endpoints.requirements),
      name: 'browse_requirements',
      checks: createStandardApiChecks(),
      requireAuth: true
    },
    {
      url: getApiUrl(config.endpoints.projectsWithPaging),
      name: 'check_related_projects',
      checks: createProjectsPerformanceChecks(),
      requireAuth: true
    }
  ];
  
  makeSequentialRequests(requests, 300);
  simulateThinkTime(1, 3);
}

function mixedApiWorkflow() {
  // Simulate user navigating between different sections
  const endpoints = [
    { url: config.endpoints.projectsWithPaging, requireAuth: true },
    { url: config.endpoints.users, requireAuth: true },
    { url: config.endpoints.requirements, requireAuth: true },
    { url: config.endpoints.health, requireAuth: false }
  ];
  
  // Random endpoint selection
  const randomEndpoint = endpoints[Math.floor(Math.random() * endpoints.length)];
  const isProjectsApi = randomEndpoint.url.includes('Projects');
  
  makeApiRequest(
    getApiUrl(randomEndpoint.url),
    'mixed_api_usage',
    isProjectsApi ? createProjectsPerformanceChecks() : createStandardApiChecks(),
    randomEndpoint.requireAuth
  );
  
  simulateThinkTime(0.5, 2);
}

function projectsIntensiveWorkflow() {
  // Focus on testing our optimized Projects API under load
  console.log('🎯 Projects-intensive workflow - testing optimization');
  
  const projectsRequests = [
    {
      url: getApiUrl(config.endpoints.projectsWithPaging),
      name: 'projects_list_intensive',
      checks: {
        ...createProjectsPerformanceChecks(),
        'optimization_target_20ms': (r) => r.timings.duration < 20,
        'optimization_target_15ms': (r) => r.timings.duration < 15
      },
      requireAuth: true
    },
    {
      url: getApiUrl(config.endpoints.projectById),
      name: 'project_details_intensive',
      checks: createProjectsPerformanceChecks(),
      requireAuth: true
    },
    {
      url: getApiUrl(`${config.endpoints.projects}?search=test`),
      name: 'project_search_intensive',
      checks: createProjectsPerformanceChecks(),
      requireAuth: true
    }
  ];
  
  makeSequentialRequests(projectsRequests, 200); // Faster requests to test optimization
  simulateThinkTime(0.5, 1.5); // Shorter think time for intensive usage
}

export function teardown(data) {
  console.log('');
  console.log('🏁 Baseline Load Test Complete');
  console.log('');
  console.log('📊 Performance Analysis:');
  console.log('   - Check Projects API response times (target: < 20ms)');
  console.log('   - Verify error rate < 1%');
  console.log('   - Confirm sustained throughput > 10 req/sec');
  console.log('   - Validate authentication success rate');
  console.log('');
  console.log('💡 Next Steps:');
  console.log('   - If performance targets met: Run stress test (npm run stress)');
  console.log('   - If Projects API > 50ms: Investigate performance regression');
  console.log('   - If error rate > 1%: Check application logs');
  console.log('   - If auth failures: Check IdentityServer logs');
  console.log('   - For detailed analysis: Review HTML report');
  console.log('');
}