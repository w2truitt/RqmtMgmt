// Debug script to test Projects page manually
const playwright = require('playwright');

async function debugProjectsPage() {
    const browser = await playwright.chromium.launch({ headless: false });
    const page = await browser.newPageAsync();
    
    console.log('Navigating to Projects page...');
    await page.goto('http://localhost:8080/projects');
    
    console.log('Waiting for page to load...');
    await page.waitForTimeout(5000); // Wait 5 seconds
    
    console.log('Checking for H3 element...');
    const h3Element = await page.locator('h3:has-text("Projects")').first();
    const isVisible = await h3Element.isVisible();
    console.log('H3 Projects element visible:', isVisible);
    
    if (!isVisible) {
        console.log('Checking page content...');
        const body = await page.locator('body').innerHTML();
        console.log('Page body content:');
        console.log(body.substring(0, 1000) + '...');
        
        console.log('Checking for errors in console...');
        page.on('console', msg => console.log('Browser console:', msg.text()));
        page.on('pageerror', exception => console.log('Page error:', exception));
    }
    
    await page.waitForTimeout(3000);
    await browser.close();
}

debugProjectsPage().catch(console.error);
