import http from 'k6/http';
import { check, sleep } from 'k6';
import { Rate, Trend } from 'k6/metrics';
import { authenticatedGet, getAuthHeaders } from './auth-helpers.js';

// Custom metrics
export const errorRate = new Rate('custom_error_rate');
export const projectsResponseTime = new Trend('projects_response_time');
export const apiResponseTime = new Trend('api_response_time');

// HTTP request options
export const httpOptions = {
  timeout: '30s',
  headers: {
    'Accept': 'application/json',
    'User-Agent': 'K6-LoadTest/1.0'
  }
};

/**
 * Makes a GET request to the specified endpoint with error handling
 * @param {string} url - The full URL to request
 * @param {string} testName - Name of the test for logging
 * @param {object} customChecks - Additional checks to perform
 * @param {boolean} requireAuth - Whether to use authentication (default: true for API endpoints)
 * @returns {object} Response object with success indicator
 */
export function makeApiRequest(url, testName = 'api_request', customChecks = {}, requireAuth = null) {
  const startTime = Date.now();
  
  // Auto-detect if authentication is needed
  if (requireAuth === null) {
    requireAuth = url.includes('/api/') && !url.includes('/health');
  }
  
  try {
    let response;
    
    if (requireAuth) {
      // Use authenticated request for API endpoints
      response = authenticatedGet(url, { 'User-Agent': 'K6-LoadTest/1.0' });
    } else {
      // Use regular request for health checks and public endpoints
      response = http.get(url, httpOptions);
    }
    
    const duration = Date.now() - startTime;
    
    // Record custom metrics
    apiResponseTime.add(duration);
    if (url.includes('/api/Projects')) {
      projectsResponseTime.add(duration);
    }
    
    // Standard checks
    const checks = {
      'status is 200': (r) => r.status === 200,
      'response time < 5000ms': (r) => r.timings.duration < 5000,
      'response has content': (r) => r.body && r.body.length > 0,
      ...customChecks
    };
    
    const checkResults = check(response, checks, { testName });
    
    // Record error rate
    errorRate.add(!checkResults);
    
    // Log authentication issues
    if (response.status === 401) {
      console.error(`❌ Authentication failed for ${testName} (${url})`);
    } else if (response.status === 403) {
      console.error(`❌ Authorization failed for ${testName} (${url})`);
    }
    
    // Log slow responses
    if (response.timings.duration > 1000) {
      console.warn(`Slow response for ${testName}: ${response.timings.duration}ms`);
    }
    
    return {
      success: response.status === 200,
      response,
      duration: response.timings.duration
    };
    
  } catch (error) {
    console.error(`Request failed for ${testName}: ${error.message}`);
    errorRate.add(1);
    return {
      success: false,
      error: error.message,
      duration: Date.now() - startTime
    };
  }
}

/**
 * Makes multiple API requests in sequence with realistic delays
 * @param {Array} requests - Array of {url, name, checks, requireAuth} objects
 * @param {number} delayMs - Delay between requests in milliseconds
 */
export function makeSequentialRequests(requests, delayMs = 100) {
  const results = [];
  
  for (const request of requests) {
    const result = makeApiRequest(
      request.url, 
      request.name, 
      request.checks || {}, 
      request.requireAuth
    );
    results.push(result);
    
    if (delayMs > 0) {
      sleep(delayMs / 1000); // K6 sleep expects seconds
    }
  }
  
  return results;
}

/**
 * Simulates realistic user behavior with think time
 * @param {number} minThinkTime - Minimum think time in seconds
 * @param {number} maxThinkTime - Maximum think time in seconds
 */
export function simulateThinkTime(minThinkTime = 1, maxThinkTime = 3) {
  const thinkTime = Math.random() * (maxThinkTime - minThinkTime) + minThinkTime;
  sleep(thinkTime);
}

/**
 * Validates Projects API response structure
 * @param {object} response - HTTP response object
 * @returns {boolean} True if response structure is valid
 */
export function validateProjectsResponse(response) {
  if (!response.body) return false;
  
  try {
    const data = JSON.parse(response.body);
    
    // Check if it's a paginated response
    if (data.items && Array.isArray(data.items)) {
      return data.items.every(project => 
        project.id && 
        project.name && 
        typeof project.status !== 'undefined'
      );
    }
    
    // Check if it's a single project
    if (data.id && data.name) {
      return true;
    }
    
    // Check if it's an array of projects
    if (Array.isArray(data)) {
      return data.every(project => 
        project.id && 
        project.name && 
        typeof project.status !== 'undefined'
      );
    }
    
    return false;
  } catch (error) {
    console.error('Failed to parse Projects API response:', error.message);
    return false;
  }
}

/**
 * Creates performance-focused checks for the optimized Projects API
 * @returns {object} Check functions object
 */
export function createProjectsPerformanceChecks() {
  return {
    'response time < 100ms': (r) => r.timings.duration < 100,
    'response time < 50ms': (r) => r.timings.duration < 50,
    'response time < 20ms': (r) => r.timings.duration < 20, // Target after optimization
    'valid projects data': validateProjectsResponse,
    'no server errors': (r) => r.status < 500,
    'content-type is JSON': (r) => r.headers['Content-Type'] && r.headers['Content-Type'].includes('application/json')
  };
}

/**
 * Creates standard API checks for general endpoints
 * @returns {object} Check functions object
 */
export function createStandardApiChecks() {
  return {
    'response time < 200ms': (r) => r.timings.duration < 200,
    'response time < 1000ms': (r) => r.timings.duration < 1000,
    'no server errors': (r) => r.status < 500,
    'response has content': (r) => r.body && r.body.length > 0
  };
}

/**
 * Logs test summary information
 * @param {string} testName - Name of the test
 * @param {string} description - Test description
 */
export function logTestStart(testName, description) {
  console.log(`🚀 Starting ${testName}: ${description}`);
  console.log(`⏰ Started at: ${new Date().toISOString()}`);
}

/**
 * Logs performance expectations
 * @param {object} expectations - Performance expectations object
 */
export function logPerformanceExpectations(expectations) {
  console.log('🎯 Performance Expectations:');
  Object.entries(expectations).forEach(([key, value]) => {
    console.log(`   ${key}: ${value}`);
  });
}