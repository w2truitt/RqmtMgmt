#!/bin/bash
# PowerShell Playwright Setup with Custom CA Certificate

# Set Node.js to use custom CA certificate
export NODE_EXTRA_CA_CERTS=/etc/ssl/node-certs/hchbcorp-root-ca.crt

# Also set for PowerShell if needed
export NODE_TLS_REJECT_UNAUTHORIZED=0  # Only as fallback - not recommended for production

# Run your PowerShell Playwright setup
echo "Environment configured for Playwright installation"
echo "NODE_EXTRA_CA_CERTS is set to: $NODE_EXTRA_CA_CERTS"

# Test the connection
echo "Testing connection to Playwright CDN..."
node -e "
const https = require('https');
const url = 'https://playwright-verizon.azureedge.net/builds/chromium/1140/chromium-linux.zip';
https.get(url, (res) => {
  console.log('✅ Connection successful - Status:', res.statusCode);
  res.destroy();
}).on('error', (err) => {
  console.log('❌ Connection failed:', err.message);
});
"

echo "Ready to run Playwright installation!"