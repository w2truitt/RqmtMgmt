import http from 'k6/http';
import { check } from 'k6';
import { config } from '../config/test-config.js';

/**
 * Authentication helper for K6 load tests.
 * Handles OAuth 2.0 client credentials flow with IdentityServer.
 */

// IdentityServer configuration
const IDENTITY_SERVER_URL = __ENV.IDENTITY_SERVER_URL || 'https://rqmtmgmt.local';
const CLIENT_ID = __ENV.CLIENT_ID || 'rqmtmgmt-backend';
const CLIENT_SECRET = __ENV.CLIENT_SECRET || 'backend-secret';
const SCOPE = __ENV.SCOPE || 'rqmtmgmt.api';

// Cache for access tokens
let cachedToken = null;
let tokenExpiry = null;

/**
 * Obtains an access token using OAuth 2.0 client credentials flow.
 * Caches the token to avoid unnecessary requests.
 * @returns {string|null} Access token or null if authentication failed or disabled
 */
export function getAccessToken() {
  // Return null if authentication is disabled
  if (!config.authEnabled) {
    return null;
  }

  // Return cached token if still valid (with 5 minute buffer)
  if (cachedToken && tokenExpiry && Date.now() < (tokenExpiry - 300000)) {
    return cachedToken;
  }

  try {
    const tokenEndpoint = `${IDENTITY_SERVER_URL}/connect/token`;
    
    const payload = {
      grant_type: 'client_credentials',
      client_id: CLIENT_ID,
      client_secret: CLIENT_SECRET,
      scope: SCOPE
    };

    const params = {
      headers: {
        'Content-Type': 'application/x-www-form-urlencoded',
        'Accept': 'application/json'
      },
      timeout: '30s'
    };

    console.log(`🔐 Requesting access token from: ${tokenEndpoint}`);
    
    const response = http.post(tokenEndpoint, payload, params);
    
    const success = check(response, {
      'token request status is 200': (r) => r.status === 200,
      'token response has access_token': (r) => {
        try {
          const data = JSON.parse(r.body);
          return data && data.access_token;
        } catch {
          return false;
        }
      }
    });

    if (!success) {
      console.error(`❌ Token request failed. Status: ${response.status}`);
      console.error(`Response: ${response.body}`);
      return null;
    }

    const tokenData = JSON.parse(response.body);
    cachedToken = tokenData.access_token;
    
    // Calculate expiry time (expires_in is in seconds)
    const expiresIn = tokenData.expires_in || 3600; // Default to 1 hour
    tokenExpiry = Date.now() + (expiresIn * 1000);
    
    console.log(`✅ Access token obtained successfully (expires in ${expiresIn}s)`);
    return cachedToken;
    
  } catch (error) {
    console.error(`❌ Error obtaining access token: ${error.message}`);
    return null;
  }
}

/**
 * Creates HTTP headers with Bearer authentication.
 * @returns {object} Headers object with Authorization header (if auth enabled)
 */
export function getAuthHeaders() {
  const baseHeaders = {
    'Accept': 'application/json',
    'Content-Type': 'application/json'
  };

  // Return basic headers if authentication is disabled
  if (!config.authEnabled) {
    return baseHeaders;
  }

  const token = getAccessToken();
  
  if (!token) {
    console.error('❌ No access token available for authentication');
    return baseHeaders;
  }

  return {
    ...baseHeaders,
    'Authorization': `Bearer ${token}`
  };
}

/**
 * Makes an authenticated HTTP GET request.
 * @param {string} url - The URL to request
 * @param {object} additionalHeaders - Additional headers to include
 * @returns {object} HTTP response object
 */
export function authenticatedGet(url, additionalHeaders = {}) {
  const headers = {
    ...getAuthHeaders(),
    ...additionalHeaders
  };

  return http.get(url, {
    headers,
    timeout: '30s'
  });
}

/**
 * Makes an authenticated HTTP POST request.
 * @param {string} url - The URL to request
 * @param {object|string} body - Request body
 * @param {object} additionalHeaders - Additional headers to include
 * @returns {object} HTTP response object
 */
export function authenticatedPost(url, body, additionalHeaders = {}) {
  const headers = {
    ...getAuthHeaders(),
    ...additionalHeaders
  };

  return http.post(url, body, {
    headers,
    timeout: '30s'
  });
}

/**
 * Validates that the authentication system is working.
 * Should be called in setup() function of tests.
 * @returns {boolean} True if authentication is working or disabled
 */
export function validateAuthentication() {
  console.log('🔍 Validating authentication system...');
  
  // If authentication is disabled, validation passes
  if (!config.authEnabled) {
    console.log('ℹ️  Authentication is disabled for load testing');
    return true;
  }
  
  const token = getAccessToken();
  if (!token) {
    console.error('❌ Authentication validation failed: Could not obtain access token');
    return false;
  }

  console.log('✅ Authentication validation successful');
  return true;
}

/**
 * Logs authentication configuration for debugging.
 */
export function logAuthConfig() {
  console.log('🔐 Authentication Configuration:');
  console.log(`   Authentication Enabled: ${config.authEnabled}`);
  
  if (config.authEnabled) {
    console.log(`   Identity Server: ${IDENTITY_SERVER_URL}`);
    console.log(`   Client ID: ${CLIENT_ID}`);
    console.log(`   Scope: ${SCOPE}`);
    console.log(`   Client Secret: ${CLIENT_SECRET ? '[CONFIGURED]' : '[MISSING]'}`);
  } else {
    console.log('   Authentication is disabled - running without auth headers');
  }
}