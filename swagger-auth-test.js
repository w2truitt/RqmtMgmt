const { chromium } = require('playwright');

/**
 * Playwright script to authenticate with Swagger UI and make an API call
 * Uses the OAuth2 Authorization Code flow configured for the swagger-ui client
 */
async function authenticateAndCallAPI() {
  const browser = await chromium.launch({ 
    headless: false, // Set to true if you want to run without UI
    slowMo: 1000 // Slow down actions for better visibility
  });
  
  const context = await browser.newContext({
    // Accept invalid certificates for local development
    ignoreHTTPSErrors: true
  });
  
  const page = await context.newPage();
  
  try {
    console.log('🌐 Navigating to Swagger UI...');
    await page.goto('https://rqmtmgmt.local/swagger/index.html');
    
    // Wait for Swagger to load
    await page.waitForSelector('.swagger-ui', { timeout: 10000 });
    console.log('✅ Swagger UI loaded successfully');
    
    // Look for the Authorize button
    console.log('🔐 Looking for Authorize button...');
    const authorizeButton = page.locator('button.btn.authorize');
    await authorizeButton.waitFor({ timeout: 5000 });
    await authorizeButton.click();
    console.log('✅ Clicked Authorize button');
    
    // Wait for the authorization modal to appear
    await page.waitForSelector('.auth-container', { timeout: 5000 });
    console.log('✅ Authorization modal opened');
    
    // Look for OAuth2 authorization section
    const oauth2Section = page.locator('.auth-container .scopes');
    await oauth2Section.waitFor({ timeout: 5000 });
    
    // Select the required scopes (openid, profile, rqmtmgmt.api)
    console.log('🎯 Selecting OAuth2 scopes...');
    const scopes = ['openid', 'profile', 'rqmtmgmt.api'];
    
    for (const scope of scopes) {
      const scopeCheckbox = page.locator(`input[type="checkbox"][data-name="${scope}"]`);
      if (await scopeCheckbox.isVisible()) {
        await scopeCheckbox.check();
        console.log(`✅ Selected scope: ${scope}`);
      }
    }
    
    // Click the Authorize button in the modal
    const modalAuthorizeButton = page.locator('.auth-btn-wrapper button.btn.authorize');
    await modalAuthorizeButton.click();
    console.log('🚀 Initiating OAuth2 flow...');
    
    // Wait for redirect to IdentityServer login page
    await page.waitForURL('**/connect/authorize**', { timeout: 10000 });
    console.log('🔄 Redirected to IdentityServer login page');
    
    // Note: At this point, you would need to handle the login process
    // Since we're using client credentials in tests, we might need to use a different approach
    
    // For demonstration, let's wait and see what happens
    await page.waitForTimeout(5000);
    
    // Take a screenshot to see the current state
    await page.screenshot({ path: 'swagger-auth-state.png' });
    console.log('📸 Screenshot saved as swagger-auth-state.png');
    
    console.log('Current URL:', page.url());
    console.log('Page title:', await page.title());
    
  } catch (error) {
    console.error('❌ Error during authentication:', error.message);
    
    // Take a screenshot of the error state
    await page.screenshot({ path: 'swagger-auth-error.png' });
    console.log('📸 Error screenshot saved as swagger-auth-error.png');
  } finally {
    await browser.close();
  }
}

/**
 * Alternative approach: Get token using client credentials flow directly
 * This mimics what the load tests do
 */
async function getTokenDirectly() {
  const https = require('https');
  const querystring = require('querystring');
  
  // Create an agent that ignores SSL certificate errors
  const agent = new https.Agent({
    rejectUnauthorized: false
  });
  
  const postData = querystring.stringify({
    grant_type: 'client_credentials',
    client_id: 'rqmtmgmt-backend',
    client_secret: 'backend-secret',
    scope: 'rqmtmgmt.api'
  });
  
  const options = {
    hostname: 'rqmtmgmt.local',
    port: 443,
    path: '/connect/token',
    method: 'POST',
    headers: {
      'Content-Type': 'application/x-www-form-urlencoded',
      'Content-Length': Buffer.byteLength(postData),
      'Accept': 'application/json'
    },
    agent: agent
  };
  
  return new Promise((resolve, reject) => {
    console.log('🔑 Requesting access token directly...');
    
    const req = https.request(options, (res) => {
      let data = '';\n      \n      res.on('data', (chunk) => {\n        data += chunk;\n      });\n      \n      res.on('end', () => {\n        if (res.statusCode === 200) {\n          const tokenData = JSON.parse(data);\n          console.log('✅ Access token obtained successfully');\n          console.log('Token type:', tokenData.token_type);\n          console.log('Expires in:', tokenData.expires_in, 'seconds');\n          console.log('Access token (first 20 chars):', tokenData.access_token.substring(0, 20) + '...');\n          resolve(tokenData.access_token);\n        } else {\n          console.error('❌ Token request failed. Status:', res.statusCode);\n          console.error('Response:', data);\n          reject(new Error(`HTTP ${res.statusCode}: ${data}`));\n        }\n      });\n    });\n    \n    req.on('error', (error) => {\n      console.error('❌ Request error:', error.message);\n      reject(error);\n    });\n    \n    req.write(postData);\n    req.end();\n  });\n}\n\n/**\n * Make an authenticated API call using the obtained token\n */\nasync function makeAuthenticatedAPICall(token) {\n  const https = require('https');\n  \n  // Create an agent that ignores SSL certificate errors\n  const agent = new https.Agent({\n    rejectUnauthorized: false\n  });\n  \n  const options = {\n    hostname: 'rqmtmgmt.local',\n    port: 443,\n    path: '/api/requirements', // Example API endpoint\n    method: 'GET',\n    headers: {\n      'Authorization': `Bearer ${token}`,\n      'Accept': 'application/json',\n      'Content-Type': 'application/json'\n    },\n    agent: agent\n  };\n  \n  return new Promise((resolve, reject) => {\n    console.log('📡 Making authenticated API call to /api/requirements...');\n    \n    const req = https.request(options, (res) => {\n      let data = '';\n      \n      res.on('data', (chunk) => {\n        data += chunk;\n      });\n      \n      res.on('end', () => {\n        console.log('📊 API Response Status:', res.statusCode);\n        console.log('📊 API Response Headers:', JSON.stringify(res.headers, null, 2));\n        \n        if (res.statusCode === 200) {\n          const responseData = JSON.parse(data);\n          console.log('✅ API call successful!');\n          console.log('📋 Response data:', JSON.stringify(responseData, null, 2));\n          resolve(responseData);\n        } else {\n          console.error('❌ API call failed. Status:', res.statusCode);\n          console.error('Response:', data);\n          reject(new Error(`HTTP ${res.statusCode}: ${data}`));\n        }\n      });\n    });\n    \n    req.on('error', (error) => {\n      console.error('❌ API request error:', error.message);\n      reject(error);\n    });\n    \n    req.end();\n  });\n}\n\n// Main execution\nasync function main() {\n  console.log('🚀 Starting authentication and API test...');\n  console.log('=' .repeat(50));\n  \n  try {\n    // Method 1: Try to get token directly (like load tests)\n    console.log('\\n📋 Method 1: Direct token request (Client Credentials)');\n    const token = await getTokenDirectly();\n    \n    // Method 2: Make an authenticated API call\n    console.log('\\n📋 Method 2: Making authenticated API call');\n    await makeAuthenticatedAPICall(token);\n    \n    // Method 3: Try Swagger UI authentication (interactive)\n    console.log('\\n📋 Method 3: Swagger UI authentication (interactive)');\n    console.log('ℹ️  This will open a browser window for manual interaction');\n    await authenticateAndCallAPI();\n    \n  } catch (error) {\n    console.error('💥 Main execution failed:', error.message);\n    process.exit(1);\n  }\n  \n  console.log('\\n🎉 All tests completed!');\n}\n\n// Run if this script is executed directly\nif (require.main === module) {\n  main();\n}\n\nmodule.exports = {\n  authenticateAndCallAPI,\n  getTokenDirectly,\n  makeAuthenticatedAPICall\n};\n