// Debug script to inspect project selector issue
console.log('=== Project Selector Debug ===');

// Wait for page to load
await page.goto('http://localhost:8080');
await page.waitForLoadState('networkidle');

console.log('Page loaded, checking for project selector...');

// Look for project selector button
const selectorButtons = await page.locator('.project-selector-container button').all();
console.log(`Found ${selectorButtons.length} project selector buttons`);

for (let i = 0; i < selectorButtons.length; i++) {
  const button = selectorButtons[i];
  const isVisible = await button.isVisible();
  const text = await button.textContent();
  console.log(`Button ${i}: visible=${isVisible}, text="${text}"`);
}

// Check for "Select Project" button specifically
const selectProjectBtn = page.locator("button:has-text('Select Project')");
const selectProjectCount = await selectProjectBtn.count();
console.log(`Select Project buttons found: ${selectProjectCount}`);

if (selectProjectCount > 0) {
  console.log('Clicking Select Project button...');
  await selectProjectBtn.first().click();
  await page.waitForTimeout(2000);
  
  // Look for dropdown
  const dropdown = page.locator('.dropdown-menu.project-dropdown');
  const dropdownVisible = await dropdown.isVisible();
  console.log(`Dropdown visible: ${dropdownVisible}`);
  
  if (dropdownVisible) {
    const items = await dropdown.locator('.dropdown-item').all();
    console.log(`Dropdown items found: ${items.length}`);
    
    for (let i = 0; i < items.length; i++) {
      const item = items[i];
      const text = await item.textContent();
      console.log(`Item ${i}: "${text}"`);
    }
  }
}

// Check browser console logs
const logs = [];
page.on('console', msg => {
  logs.push(`${msg.type()}: ${msg.text()}`);
});

console.log('=== Browser Console Logs ===');
logs.forEach(log => console.log(log));

// Check network requests
const requests = [];
page.on('request', request => {
  requests.push(`${request.method()} ${request.url()}`);
});

console.log('=== Network Requests ===');
requests.forEach(req => console.log(req));
