// Configuration for all load tests
export const config = {
  // API Configuration
  apiBaseUrl: __ENV.API_BASE_URL || 'https://rqmtmgmt.local',
  apiTimeout: __ENV.API_TIMEOUT || '30s',
  
  // Authentication Configuration
  authEnabled: __ENV.AUTH_ENABLED === 'true', // Enable auth when explicitly set to 'true'
  
  // Performance Thresholds
  maxResponseTime: parseInt(__ENV.MAX_RESPONSE_TIME) || 100, // milliseconds
  errorRateThreshold: parseFloat(__ENV.ERROR_RATE_THRESHOLD) || 0.01, // 1%
  minThroughput: parseInt(__ENV.MIN_THROUGHPUT) || 50, // requests/second
  
  // Test Durations
  smokeDuration: __ENV.SMOKE_DURATION || '30s',
  baselineDuration: __ENV.BASELINE_DURATION || '5m',
  stressDuration: __ENV.STRESS_DURATION || '7m',
  spikeDuration: __ENV.SPIKE_DURATION || '4m30s',
  enduranceDuration: __ENV.ENDURANCE_DURATION || '15m',
  projectsFocusedDuration: __ENV.PROJECTS_DURATION || '2m',
  
  // API Endpoints
  endpoints: {
    projects: '/api/Projects',
    projectsWithPaging: '/api/Projects?page=1&pageSize=10',
    projectById: '/api/Projects/1',
    users: '/api/Users',
    requirements: '/api/Requirements',
    testCases: '/api/TestCases',
    testSuites: '/api/TestSuites',
    health: '/health'
  },
  
  // Load Patterns
  loadPatterns: {
    smoke: {
      vus: 2,
      duration: '30s'
    },
    baseline: {
      stages: [
        { duration: '1m', target: 5 },   // Ramp up
        { duration: '3m', target: 10 },  // Normal load
        { duration: '1m', target: 0 }    // Ramp down
      ]
    },
    stress: {
      stages: [
        { duration: '1m', target: 5 },   // Start low
        { duration: '2m', target: 15 },  // Increase
        { duration: '2m', target: 30 },  // Higher load
        { duration: '1m', target: 50 },  // Peak load
        { duration: '1m', target: 0 }    // Ramp down
      ]
    },
    spike: {
      stages: [
        { duration: '1m', target: 5 },   // Normal load
        { duration: '30s', target: 50 }, // Sudden spike
        { duration: '1m', target: 5 },   // Back to normal
        { duration: '1m', target: 50 },  // Another spike
        { duration: '1m', target: 0 }    // Ramp down
      ]
    },
    endurance: {
      stages: [
        { duration: '2m', target: 8 },   // Ramp up
        { duration: '11m', target: 8 },  // Sustained load
        { duration: '2m', target: 0 }    // Ramp down
      ]
    },
    projectsFocused: {
      stages: [
        { duration: '30s', target: 10 }, // Ramp up
        { duration: '1m', target: 20 },  // High load on Projects API
        { duration: '30s', target: 0 }   // Ramp down
      ]
    }
  },
  
  // Common thresholds for all tests
  commonThresholds: {
    http_req_duration: ['p(95)<100'], // 95% of requests under 100ms
    http_req_failed: ['rate<0.01'],   // Error rate under 1%
    http_reqs: ['rate>10']            // Minimum 10 requests/second
  },
  
  // Strict thresholds for performance-critical tests
  strictThresholds: {
    http_req_duration: ['p(95)<50', 'p(99)<100'], // Stricter response times
    http_req_failed: ['rate<0.005'],              // Error rate under 0.5%
    http_reqs: ['rate>20']                        // Higher minimum throughput
  }
};

// Helper function to get full URL
export function getApiUrl(endpoint) {
  return `${config.apiBaseUrl}${endpoint}`;
}

// Helper function to get appropriate thresholds
export function getThresholds(strict = false) {
  return strict ? config.strictThresholds : config.commonThresholds;
}