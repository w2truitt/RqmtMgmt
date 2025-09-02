import { check, sleep } from 'k6';
import { config, getApiUrl, getThresholds } from '../config/test-config.js';
import { 
  makeApiRequest, 
  logTestStart, 
  logPerformanceExpectations,
  createProjectsPerformanceChecks,
  createStandardApiChecks 
} from '../utils/test-helpers.js';

// Test configuration for spike testing
export let options = {
  stages: config.loadPatterns.spike.stages,
  thresholds: {
    http_req_duration: ['p(95)<300'], // Allow higher response times during spikes
    http_req_failed: ['rate<0.1'],    // Allow 10% error rate during spikes
    http_reqs: ['rate>3']             // Minimum throughput during recovery
  },
  
  // Spike test configuration
  noConnectionReuse: false,
  userAgent: 'K6-SpikeTest/1.0',
  
  // Detailed reporting for spike analysis
  summaryTrendStats: ['avg', 'min', 'med', 'max', 'p(90)', 'p(95)', 'p(99)'],
  summaryTimeUnit: 'ms'
};

export function setup() {
  logTestStart('Spike Load Test', 'Test system response to sudden traffic increases');
  logPerformanceExpectations({
    'Duration': '4.5 minutes',
    'Load Pattern': 'Normal → Spike → Normal → Spike → Recovery',
    'Normal Load': '5 users',
    'Spike Load': '50 users (10x increase)',
    'Spike Duration': '30 seconds each',
    'Target': 'System stability during traffic spikes',
    'Recovery': 'Quick return to normal performance',
    'Focus': 'Resilience and auto-scaling behavior'
  });
  
  console.log('🔍 Spike Test Pattern:');
  console.log('   Stage 1 (1m): 5 users - Normal baseline');
  console.log('   Stage 2 (30s): 50 users - First spike (10x)');
  console.log('   Stage 3 (1m): 5 users - Recovery period');
  console.log('   Stage 4 (30s): 50 users - Second spike');
  console.log('   Stage 5 (1m): 0 users - Final recovery');
  console.log('');
  console.log('🎯 Key Resilience Metrics:');
  console.log('   - Response time during spikes');
  console.log('   - Error rate during sudden load increases');
  console.log('   - Recovery time to normal performance');
  console.log('   - Projects API stability (our optimization)');
  console.log('');
  
  // Pre-test verification
  const healthCheck = makeApiRequest(getApiUrl(config.endpoints.health), 'pre_spike_health');
  if (!healthCheck.success) {
    throw new Error('❌ API health check failed - cannot proceed with spike test');
  }
  console.log('✅ API is healthy, starting spike test');
  console.log('');
}

export default function() {
  const currentPhase = getCurrentPhase();
  
  switch (currentPhase) {
    case 'baseline':
      baselinePhase();
      break;
    case 'spike':
      spikePhase();
      break;
    case 'recovery':
      recoveryPhase();
      break;
    default:
      normalPhase();
  }
}

function getCurrentPhase() {
  // Determine current phase based on virtual user count or time
  const vu = __VU;
  const iter = __ITER;
  
  // Rough estimation - in real scenario you'd use more sophisticated logic
  if (iter < 10) return 'baseline';
  if (iter < 30) return 'spike';
  if (iter < 50) return 'recovery';
  if (iter < 70) return 'spike';
  return 'recovery';
}

function baselinePhase() {
  // Normal load - establish baseline before spike
  const result = makeApiRequest(
    getApiUrl(config.endpoints.projectsWithPaging),
    'spike_baseline_projects',
    {
      ...createProjectsPerformanceChecks(),
      'pre_spike_baseline': (r) => r.timings.duration < 50
    }
  );
  
  if (result.success) {
    console.log(`📊 Baseline Projects API: ${result.duration}ms`);
  }
  
  sleep(1);
}

function spikePhase() {
  // Sudden high load - test system response to traffic spike
  console.log('⚡ SPIKE PHASE - Testing sudden load increase');
  
  // Focus on critical endpoints during spike
  const endpoints = [
    {
      url: getApiUrl(config.endpoints.projectsWithPaging),
      name: 'spike_projects',
      checks: {
        'survives_spike': (r) => r.status === 200,
        'spike_response_time': (r) => r.timings.duration < 1000,
        'optimization_resilient': (r) => r.timings.duration < 200,
        'no_server_crash': (r) => r.status < 500
      }
    },
    {
      url: getApiUrl(config.endpoints.health),
      name: 'spike_health',
      checks: {
        'health_during_spike': (r) => r.status === 200,
        'health_responsive': (r) => r.timings.duration < 500
      }
    }
  ];
  
  // Random endpoint to distribute spike load
  const endpoint = endpoints[Math.floor(Math.random() * endpoints.length)];
  const result = makeApiRequest(endpoint.url, endpoint.name, endpoint.checks);
  
  // Monitor spike impact
  if (endpoint.name === 'spike_projects') {
    if (result.duration > 500) {
      console.warn(`⚠️  Projects API spike impact: ${result.duration}ms`);
    } else if (result.duration < 100) {
      console.log(`✅ Projects API resilient during spike: ${result.duration}ms`);
    }
  }
  
  sleep(0.1); // Minimal sleep during spike
}

function recoveryPhase() {
  // Recovery period - test system return to normal
  const result = makeApiRequest(
    getApiUrl(config.endpoints.projectsWithPaging),
    'spike_recovery_projects',
    {
      ...createProjectsPerformanceChecks(),
      'recovery_performance': (r) => r.timings.duration < 100,
      'back_to_baseline': (r) => r.timings.duration < 50
    }
  );
  
  if (result.success) {
    if (result.duration < 50) {
      console.log(`✅ Recovery complete: ${result.duration}ms (back to baseline)`);
    } else if (result.duration < 100) {
      console.log(`🔄 Recovering: ${result.duration}ms (improving)`);
    } else {
      console.warn(`⚠️  Slow recovery: ${result.duration}ms`);
    }
  }
  
  sleep(0.5);
}

function normalPhase() {
  // Default behavior for transitional periods
  makeApiRequest(
    getApiUrl(config.endpoints.projectsWithPaging),
    'spike_normal_projects',
    createProjectsPerformanceChecks()
  );
  
  sleep(0.8);
}

export function teardown(data) {
  console.log('');
  console.log('🏁 Spike Load Test Complete');
  console.log('');
  console.log('📊 Spike Test Analysis:');
  console.log('   - Review response time spikes (normal vs spike phases)');
  console.log('   - Check error rate during 10x load increases');
  console.log('   - Verify system recovery time to baseline performance');
  console.log('   - Assess Projects API resilience during spikes');
  console.log('');
  console.log('🔍 Key Resilience Questions:');
  console.log('   - How did response times change during spikes?');
  console.log('   - Did the system maintain availability during 50-user spikes?');
  console.log('   - How quickly did performance return to baseline?');
  console.log('   - Did Projects API optimization hold during spikes?');
  console.log('');
  console.log('💡 Next Steps:');
  console.log('   - If spike test passed: Run endurance test (npm run endurance)');
  console.log('   - If high error rates during spikes: Implement auto-scaling');
  console.log('   - If slow recovery: Investigate connection pooling');
  console.log('   - Document spike handling capacity for infrastructure planning');
  console.log('');
}