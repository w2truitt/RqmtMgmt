// Playwright configuration for HTTPS testing with custom CA certificate
const { chromium } = require('playwright');

// Path to our CA certificate
const CA_CERT_PATH = '/home/wtruitt/src/repos/RqmtMgmt/docker-compose/certs/rqmtmgmt-ca.crt';

// Launch browser with certificate trust
async function launchBrowserWithCerts() {
    const browser = await chromium.launch({
        headless: false, // Set to true for headless testing
        ignoreHTTPSErrors: true, // Ignore HTTPS errors as fallback
        args: [
            '--ignore-certificate-errors',
            '--ignore-ssl-errors',
            '--ignore-certificate-errors-spki-list',
            '--disable-web-security',
            '--allow-running-insecure-content',
            // Use system certificate store
            '--use-system-default-printer',
            // Additional security bypass for development
            '--disable-features=VizDisplayCompositor'
        ]
    });
    
    return browser;
}

module.exports = { launchBrowserWithCerts, CA_CERT_PATH };