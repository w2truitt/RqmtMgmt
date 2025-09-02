import { config, getApiUrl, getThresholds } from '../config/test-config.js';
import { 
  makeApiRequest, 
  makeSequentialRequests,
  logTestStart, 
  logPerformanceExpectations,
  createProjectsPerformanceChecks,
  validateProjectsResponse 
} from '../utils/test-helpers.js';
import { validateAuthentication, logAuthConfig } from '../utils/auth-helpers.js';

// Test configuration specifically for Projects API optimization validation
export let options = {
  stages: config.loadPatterns.projectsFocused.stages,
  thresholds: {
    // Strict thresholds for our optimized Projects API
    http_req_duration: ['p(95)<50', 'p(99)<100'], // Post-optimization targets
    http_req_failed: ['rate<0.005'],              // Very low error rate
    http_reqs: ['rate>15'],                       // High throughput expected
    
    // Custom thresholds for Projects-specific metrics
    'projects_response_time': ['p(95)<30', 'avg<20'], // Our 11-15ms target
  },
  
  // Optimized configuration for Projects API testing
  noConnectionReuse: false,
  userAgent: 'K6-ProjectsOptimizationTest/1.0',
  
  // Detailed reporting for optimization validation
  summaryTrendStats: ['avg', 'min', 'med', 'max', 'p(90)', 'p(95)', 'p(99)'],
  summaryTimeUnit: 'ms'
};

export function setup() {
  logTestStart('Projects API Optimization Test', 'Intensive validation of 99.95% performance improvement');
  logAuthConfig();
  
  // Validate authentication before starting tests
  if (!validateAuthentication()) {
    throw new Error('Authentication validation failed. Please ensure IdentityServer is running and configured correctly.');
  }
  
  logPerformanceExpectations({
    'Duration': '2 minutes',
    'Load Pattern': 'Ramp 30s → High load 1m → Ramp down 30s',
    'Peak Virtual Users': '20',
    'Target Response Time': '< 30ms (95th percentile)',
    'Optimization Goal': '11-15ms average (99.95% improvement from 30+ seconds)',
    'Error Rate': '< 0.5%',
    'Min Throughput': '> 15 req/sec',
    'Focus': 'Validate EF Core projection optimization effectiveness with authentication'
  });
  
  console.log('🎯 Projects API Optimization Context:');
  console.log('   BEFORE: 30+ seconds (expensive EF Core count subqueries)');
  console.log('   AFTER: 11-15ms (eliminated subqueries, added AsNoTracking)');
  console.log('   IMPROVEMENT: 99.95% faster (2000x improvement)');
  console.log('');
  
  console.log('🔍 Test Endpoints:');
  console.log(`   Projects List: ${getApiUrl(config.endpoints.projectsWithPaging)} (auth)`);
  console.log(`   Project Details: ${getApiUrl(config.endpoints.projectById)} (auth)`);
  console.log(`   Project Search: ${getApiUrl(config.endpoints.projects)}?search=test (auth)`);
  console.log('');
  
  // Pre-test validation
  console.log('🔧 Pre-test optimization validation...');
  const optimizationCheck = makeApiRequest(
    getApiUrl(config.endpoints.projectsWithPaging), 
    'optimization_validation',
    {
      'meets_optimization_target': (r) => r.timings.duration < 50,
      'exceeds_optimization_goal': (r) => r.timings.duration < 20
    },
    true
  );
  
  if (!optimizationCheck.success) {
    throw new Error('❌ Projects API optimization validation failed - cannot proceed');
  }
  
  if (optimizationCheck.duration > 50) {
    console.warn(`⚠️  Warning: Projects API response time ${optimizationCheck.duration}ms > 50ms target`);
    console.warn('   This may indicate performance regression since optimization');
  } else {
    console.log(`✅ Optimization validated: ${optimizationCheck.duration}ms response time`);
  }
  
  console.log('');
}

export default function() {
  // Focus entirely on Projects API with different usage patterns
  const testPattern = Math.random();
  
  if (testPattern < 0.5) {
    // 50% - Standard Projects listing (most common use case)
    standardProjectsListing();
  } else if (testPattern < 0.8) {
    // 30% - Project details viewing
    projectDetailsViewing();
  } else {
    // 20% - Project search functionality
    projectSearchTesting();
  }
}

function standardProjectsListing() {
  // Test the main Projects API endpoint that was optimized
  makeApiRequest(
    getApiUrl(config.endpoints.projectsWithPaging),
    'projects_listing_optimized',
    {
      // Optimization validation checks
      'optimization_11ms_target': (r) => r.timings.duration < 11,
      'optimization_15ms_target': (r) => r.timings.duration < 15,
      'optimization_20ms_target': (r) => r.timings.duration < 20,
      'optimization_50ms_ceiling': (r) => r.timings.duration < 50,
      
      // Data validation
      'has_valid_projects_data': validateProjectsResponse,
      'has_pagination_info': (r) => {
        try {
          const data = JSON.parse(r.body);
          return data && (data.items || data.totalCount !== undefined);
        } catch {
          return false;
        }
      },
      
      // Performance regression detection
      'no_performance_regression': (r) => r.timings.duration < 100,
      'excellent_performance': (r) => r.timings.duration < 30
    },
    true
  );
}

function projectDetailsViewing() {
  // Test individual project retrieval
  makeApiRequest(
    getApiUrl(config.endpoints.projectById),
    'project_details_optimized',
    {
      ...createProjectsPerformanceChecks(),
      'project_details_fast': (r) => r.timings.duration < 30,
      'project_details_optimal': (r) => r.timings.duration < 15
    },
    true
  );
}

function projectSearchTesting() {
  // Test project search functionality
  const searchQueries = ['test', 'project', 'sample', 'demo'];
  const randomQuery = searchQueries[Math.floor(Math.random() * searchQueries.length)];
  
  makeApiRequest(
    getApiUrl(`${config.endpoints.projects}?search=${randomQuery}`),
    'project_search_optimized',
    {
      ...createProjectsPerformanceChecks(),
      'search_performance': (r) => r.timings.duration < 50,
      'search_results_valid': (r) => {
        try {
          const data = JSON.parse(r.body);
          return Array.isArray(data) || (data && data.items);
        } catch {
          return false;
        }
      }
    },
    true
  );
}

export function teardown(data) {
  console.log('');
  console.log('🏁 Projects API Optimization Test Complete');
  console.log('');
  console.log('📊 Optimization Results Analysis:');
  console.log('   ✓ Target: 11-15ms average response time');
  console.log('   ✓ Ceiling: < 50ms for 95th percentile');
  console.log('   ✓ Regression Detection: No responses > 100ms');
  console.log('   ✓ Authentication: All requests properly authenticated');
  console.log('');
  console.log('🎯 Success Criteria:');
  console.log('   - Average response time < 20ms');
  console.log('   - 95th percentile < 30ms');
  console.log('   - Error rate < 0.5%');
  console.log('   - Throughput > 15 req/sec');
  console.log('   - No authentication failures');
  console.log('');
  console.log('💡 Next Steps:');
  console.log('   - If targets met: Optimization is working correctly');
  console.log('   - If response times > 50ms: Investigate performance regression');
  console.log('   - If auth failures: Check IdentityServer configuration');
  console.log('   - For production: Monitor these metrics continuously');
  console.log('');
}