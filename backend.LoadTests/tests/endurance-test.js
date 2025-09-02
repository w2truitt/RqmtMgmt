import { check, sleep } from 'k6';
import { config, getApiUrl, getThresholds } from '../config/test-config.js';
import { 
  makeApiRequest, 
  logTestStart, 
  logPerformanceExpectations,
  createProjectsPerformanceChecks,
  createStandardApiChecks 
} from '../utils/test-helpers.js';

// Test configuration for endurance testing
export let options = {
  stages: config.loadPatterns.endurance.stages,
  thresholds: {
    http_req_duration: ['p(95)<150'],     // Stable performance over time
    http_req_failed: ['rate<0.02'],       // Low error rate for sustained load
    http_reqs: ['rate>5'],                // Consistent throughput
    // Memory leak detection (response times shouldn't increase over time)
    http_req_duration: ['p(95)<150', 'avg<75']
  },
  
  // Endurance test configuration
  noConnectionReuse: false,
  userAgent: 'K6-EnduranceTest/1.0',
  
  // Extended reporting for trend analysis
  summaryTrendStats: ['avg', 'min', 'med', 'max', 'p(90)', 'p(95)', 'p(99)'],
  summaryTimeUnit: 'ms'
};

export function setup() {
  logTestStart('Endurance Load Test', 'Long-term stability test with sustained moderate load');
  logPerformanceExpectations({
    'Duration': '15 minutes',
    'Load Pattern': 'Ramp 2m → Sustain 11m → Ramp down 2m',
    'Sustained Users': '8',
    'Target Response Time': '< 150ms (95th percentile, stable)',
    'Error Rate': '< 2% (sustained)',
    'Min Throughput': '> 5 req/sec (consistent)',
    'Focus': 'Memory leaks, performance degradation, long-term stability'
  });
  
  console.log('🔍 Endurance Test Strategy:');
  console.log('   Stage 1 (2m): Ramp to 8 users');
  console.log('   Stage 2 (11m): Sustain 8 users - Core endurance phase');
  console.log('   Stage 3 (2m): Ramp down to 0 - Clean shutdown');
  console.log('');
  console.log('🎯 Stability Metrics to Monitor:');
  console.log('   - Response time trends (should remain stable)');
  console.log('   - Memory usage patterns (no leaks)');
  console.log('   - Error rate consistency');
  console.log('   - Projects API optimization durability');
  console.log('');
  
  // Pre-test verification and baseline establishment
  console.log('🔧 Establishing performance baseline...');
  const baselineResults = [];
  
  for (let i = 0; i < 3; i++) {
    const result = makeApiRequest(
      getApiUrl(config.endpoints.projectsWithPaging),
      'endurance_baseline',
      createProjectsPerformanceChecks()
    );
    if (result.success) {
      baselineResults.push(result.duration);
    }
    sleep(1);
  }
  
  if (baselineResults.length > 0) {
    const avgBaseline = baselineResults.reduce((a, b) => a + b) / baselineResults.length;
    console.log(`✅ Baseline Projects API: ${avgBaseline.toFixed(1)}ms (avg of ${baselineResults.length} requests)`);
    console.log('📊 Starting endurance test - monitoring for performance degradation');
  } else {
    throw new Error('❌ Could not establish baseline - API may be unavailable');
  }
  console.log('');
}

let iterationCount = 0;
let performanceHistory = [];

export default function() {
  iterationCount++;
  
  // Determine test phase
  const phase = getEndurancePhase();
  
  switch (phase) {
    case 'rampup':
      rampUpPhase();
      break;
    case 'sustain':
      sustainedLoadPhase();
      break;
    case 'rampdown':
      rampDownPhase();
      break;
  }
}

function getEndurancePhase() {
  // Rough phase detection based on iteration count
  // In production, you'd use more sophisticated timing
  if (iterationCount < 50) return 'rampup';
  if (iterationCount < 400) return 'sustain';  // Main endurance phase
  return 'rampdown';
}

function rampUpPhase() {
  // Ramp up phase - monitor initial performance
  const result = makeApiRequest(
    getApiUrl(config.endpoints.projectsWithPaging),
    'endurance_rampup_projects',
    {
      ...createProjectsPerformanceChecks(),
      'rampup_stability': (r) => r.timings.duration < 100
    }
  );
  
  if (result.success) {
    console.log(`📈 Ramp-up Projects API: ${result.duration}ms`);
  }
  
  sleep(1);
}

function sustainedLoadPhase() {
  // Main endurance phase - comprehensive monitoring
  const testCycle = iterationCount % 4;
  
  let endpoint, testName, checks;
  
  switch (testCycle) {
    case 0:
      // Primary focus: Projects API (our optimization)
      endpoint = config.endpoints.projectsWithPaging;
      testName = 'endurance_sustain_projects';
      checks = {
        ...createProjectsPerformanceChecks(),
        'endurance_optimization': (r) => r.timings.duration < 50,
        'no_degradation': (r) => r.timings.duration < 100,
        'memory_stability': (r) => r.timings.duration < 200
      };
      break;
      
    case 1:
      // Secondary: Users API
      endpoint = config.endpoints.users;
      testName = 'endurance_sustain_users';
      checks = createStandardApiChecks();
      break;
      
    case 2:
      // Tertiary: Requirements API
      endpoint = config.endpoints.requirements;
      testName = 'endurance_sustain_requirements';
      checks = createStandardApiChecks();
      break;
      
    case 3:
      // Health monitoring
      endpoint = config.endpoints.health;
      testName = 'endurance_sustain_health';
      checks = {
        'health_stable': (r) => r.status === 200,
        'health_fast': (r) => r.timings.duration < 50
      };
      break;
  }
  
  const result = makeApiRequest(getApiUrl(endpoint), testName, checks);
  
  // Track performance trends for Projects API
  if (testName === 'endurance_sustain_projects' && result.success) {
    performanceHistory.push({
      iteration: iterationCount,
      duration: result.duration,
      timestamp: Date.now()
    });
    
    // Analyze trends every 50 iterations
    if (performanceHistory.length % 50 === 0) {
      analyzePerfomanceTrends();
    }
    
    // Alert on significant degradation
    if (result.duration > 100) {
      console.warn(`⚠️  Projects API degradation detected: ${result.duration}ms at iteration ${iterationCount}`);
    } else if (result.duration < 30) {
      console.log(`✅ Projects API performing excellently: ${result.duration}ms`);
    }
  }
  
  sleep(0.8); // Sustained moderate load
}

function rampDownPhase() {
  // Ramp down phase - verify clean shutdown
  const result = makeApiRequest(
    getApiUrl(config.endpoints.projectsWithPaging),
    'endurance_rampdown_projects',
    {
      ...createProjectsPerformanceChecks(),
      'clean_shutdown': (r) => r.timings.duration < 100
    }
  );
  
  if (result.success) {
    console.log(`📉 Ramp-down Projects API: ${result.duration}ms`);
  }
  
  sleep(1.5);
}

function analyzePerfomanceTrends() {
  if (performanceHistory.length < 10) return;
  
  const recent = performanceHistory.slice(-10);
  const older = performanceHistory.slice(-20, -10);
  
  if (older.length === 0) return;
  
  const recentAvg = recent.reduce((sum, p) => sum + p.duration, 0) / recent.length;
  const olderAvg = older.reduce((sum, p) => sum + p.duration, 0) / older.length;
  
  const degradation = ((recentAvg - olderAvg) / olderAvg) * 100;
  
  if (degradation > 20) {
    console.warn(`⚠️  Performance degradation detected: ${degradation.toFixed(1)}% increase (${olderAvg.toFixed(1)}ms → ${recentAvg.toFixed(1)}ms)`);
  } else if (degradation < -10) {
    console.log(`✅ Performance improvement: ${Math.abs(degradation).toFixed(1)}% decrease (${olderAvg.toFixed(1)}ms → ${recentAvg.toFixed(1)}ms)`);
  } else {
    console.log(`📊 Performance stable: ${recentAvg.toFixed(1)}ms (${degradation.toFixed(1)}% change)`);
  }
}

export function teardown(data) {
  console.log('');
  console.log('🏁 Endurance Load Test Complete');
  console.log('');
  
  // Final performance analysis
  if (performanceHistory.length > 0) {
    const first10 = performanceHistory.slice(0, 10);
    const last10 = performanceHistory.slice(-10);
    
    if (first10.length > 0 && last10.length > 0) {
      const startAvg = first10.reduce((sum, p) => sum + p.duration, 0) / first10.length;
      const endAvg = last10.reduce((sum, p) => sum + p.duration, 0) / last10.length;
      const totalChange = ((endAvg - startAvg) / startAvg) * 100;
      
      console.log('📊 Endurance Performance Summary:');
      console.log(`   Start Average: ${startAvg.toFixed(1)}ms`);
      console.log(`   End Average: ${endAvg.toFixed(1)}ms`);
      console.log(`   Total Change: ${totalChange.toFixed(1)}%`);
      
      if (Math.abs(totalChange) < 10) {
        console.log('   ✅ Performance remained stable throughout test');
      } else if (totalChange > 10) {
        console.log('   ⚠️  Performance degraded over time - investigate memory leaks');
      } else {
        console.log('   📈 Performance improved over time - system warming up');
      }
    }
  }
  
  console.log('');
  console.log('🔍 Endurance Analysis Questions:');
  console.log('   - Did response times remain stable over 15 minutes?');
  console.log('   - Were there any memory leak indicators?');
  console.log('   - Did Projects API optimization hold throughout?');
  console.log('   - Was error rate consistently low?');
  console.log('');
  console.log('💡 Next Steps:');
  console.log('   - If endurance test passed: System is production-ready');
  console.log('   - If performance degraded: Investigate memory leaks');
  console.log('   - If error rate increased: Check connection pool settings');
  console.log('   - Document sustained load capacity for operations');
  console.log('');
}