import { check, sleep } from 'k6';
import { config, getApiUrl, getThresholds } from '../config/test-config.js';
import { 
  makeApiRequest, 
  logTestStart, 
  logPerformanceExpectations,
  createProjectsPerformanceChecks,
  createStandardApiChecks 
} from '../utils/test-helpers.js';

// Test configuration with relaxed thresholds for stress testing
export let options = {
  stages: config.loadPatterns.stress.stages,
  thresholds: {
    http_req_duration: ['p(95)<200', 'p(99)<500'], // Relaxed for stress test
    http_req_failed: ['rate<0.05'],                // Allow 5% error rate under stress
    http_reqs: ['rate>5']                          // Minimum throughput under stress
  },
  
  // Stress test configuration
  noConnectionReuse: false,
  userAgent: 'K6-StressTest/1.0',
  
  // Detailed reporting for stress analysis
  summaryTrendStats: ['avg', 'min', 'med', 'max', 'p(90)', 'p(95)', 'p(99)', 'p(99.9)'],
  summaryTimeUnit: 'ms'
};

export function setup() {
  logTestStart('Stress Load Test', 'Gradually increase load to find system breaking points');
  logPerformanceExpectations({
    'Duration': '7 minutes',
    'Load Pattern': '5 → 15 → 30 → 50 users (gradual increase)',
    'Peak Virtual Users': '50',
    'Target Response Time': '< 200ms (95th percentile under stress)',
    'Acceptable Error Rate': '< 5% (higher tolerance for stress test)',
    'Min Throughput': '> 5 req/sec (under peak load)',
    'Focus': 'Find maximum sustainable load and breaking points'
  });
  
  console.log('🔍 Stress Test Strategy:');
  console.log('   Stage 1 (1m): 5 users - Baseline performance');
  console.log('   Stage 2 (2m): 15 users - Moderate load');
  console.log('   Stage 3 (2m): 30 users - High load');
  console.log('   Stage 4 (1m): 50 users - Peak stress');
  console.log('   Stage 5 (1m): 0 users - Recovery');
  console.log('');
  console.log('🎯 Key Metrics to Watch:');
  console.log('   - Projects API response time (optimized endpoint)');
  console.log('   - Error rate increase under load');
  console.log('   - System recovery after peak load');
  console.log('');
  
  // Pre-test verification
  const healthCheck = makeApiRequest(getApiUrl(config.endpoints.health), 'pre_stress_health');
  if (!healthCheck.success) {
    throw new Error('❌ API health check failed - cannot proceed with stress test');
  }
  console.log('✅ API is healthy, starting stress test');
  console.log('');
}

export default function() {
  const currentStage = getCurrentStage();
  
  // Adjust behavior based on current load stage
  if (currentStage <= 1) {
    // Low load - focus on baseline performance
    baselineStressWorkflow();
  } else if (currentStage <= 3) {
    // Medium to high load - mixed workflows
    mixedStressWorkflow();
  } else {
    // Peak load - intensive testing
    peakStressWorkflow();
  }
}

function getCurrentStage() {
  // Approximate current stage based on test duration
  // This is a rough estimate since K6 doesn't provide direct stage info
  const elapsed = Date.now() - __ITER_START__;
  if (elapsed < 60000) return 1;      // First minute
  if (elapsed < 180000) return 2;     // Minutes 1-3
  if (elapsed < 300000) return 3;     // Minutes 3-5
  if (elapsed < 360000) return 4;     // Minutes 5-6
  return 5;                           // Final minute
}

function baselineStressWorkflow() {
  // Low load - establish baseline performance
  const projectsResult = makeApiRequest(
    getApiUrl(config.endpoints.projectsWithPaging),
    'stress_baseline_projects',
    {
      ...createProjectsPerformanceChecks(),
      'baseline_performance_20ms': (r) => r.timings.duration < 20,
      'baseline_performance_50ms': (r) => r.timings.duration < 50
    }
  );
  
  // Log baseline performance for comparison
  if (projectsResult.success && projectsResult.duration < 50) {
    console.log(`✅ Baseline Projects API: ${projectsResult.duration}ms`);
  }
  
  sleep(1); // Normal think time
}

function mixedStressWorkflow() {
  // Medium load - test multiple endpoints
  const endpoints = [
    {
      url: getApiUrl(config.endpoints.projectsWithPaging),
      name: 'stress_projects',
      checks: createProjectsPerformanceChecks()
    },
    {
      url: getApiUrl(config.endpoints.users),
      name: 'stress_users',
      checks: createStandardApiChecks()
    },
    {
      url: getApiUrl(config.endpoints.requirements),
      name: 'stress_requirements',
      checks: createStandardApiChecks()
    }
  ];
  
  // Random endpoint selection
  const endpoint = endpoints[Math.floor(Math.random() * endpoints.length)];
  const result = makeApiRequest(endpoint.url, endpoint.name, endpoint.checks);
  
  // Monitor performance degradation
  if (endpoint.name === 'stress_projects' && result.duration > 100) {
    console.warn(`⚠️  Projects API degradation under load: ${result.duration}ms`);
  }
  
  sleep(0.5); // Reduced think time under load
}

function peakStressWorkflow() {
  // Peak load - intensive testing to find breaking point
  console.log('🔥 Peak stress - testing system limits');
  
  // Focus on our optimized Projects API under extreme load
  const projectsResult = makeApiRequest(
    getApiUrl(config.endpoints.projectsWithPaging),
    'stress_peak_projects',
    {
      'survives_peak_load': (r) => r.status === 200,
      'response_time_under_stress': (r) => r.timings.duration < 500,
      'no_server_errors': (r) => r.status < 500,
      'optimization_holds': (r) => r.timings.duration < 100 // Check if optimization holds
    }
  );
  
  // Also test a secondary endpoint
  makeApiRequest(
    getApiUrl(config.endpoints.health),
    'stress_peak_health',
    {
      'health_survives_stress': (r) => r.status === 200,
      'health_responds_quickly': (r) => r.timings.duration < 100
    }
  );
  
  // Log critical performance issues
  if (!projectsResult.success) {
    console.error('❌ Projects API failed under peak stress');
  } else if (projectsResult.duration > 200) {
    console.warn(`⚠️  Projects API under severe stress: ${projectsResult.duration}ms`);
  }
  
  sleep(0.1); // Minimal think time under peak stress
}

export function teardown(data) {
  console.log('');
  console.log('🏁 Stress Load Test Complete');
  console.log('');
  console.log('📊 Stress Test Analysis:');
  console.log('   - Review response time degradation under load');
  console.log('   - Check error rate progression (5 → 15 → 30 → 50 users)');
  console.log('   - Verify Projects API optimization held under stress');
  console.log('   - Confirm system recovery after peak load');
  console.log('');
  console.log('🔍 Key Questions to Answer:');
  console.log('   - At what load level did response times exceed 100ms?');
  console.log('   - Did Projects API maintain < 50ms under moderate load?');
  console.log('   - What was the maximum sustainable load before errors?');
  console.log('   - Did the system recover quickly after peak stress?');
  console.log('');
  console.log('💡 Next Steps:');
  console.log('   - If stress test passed: Run spike test (npm run spike)');
  console.log('   - If high error rates: Investigate bottlenecks');
  console.log('   - If Projects API > 200ms: Check for performance regression');
  console.log('   - Document maximum sustainable load for capacity planning');
  console.log('');
}