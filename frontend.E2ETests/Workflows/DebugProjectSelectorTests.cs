using frontend.E2ETests.PageObjects;
using Microsoft.Playwright;
using Xunit;

namespace frontend.E2ETests.Workflows
{
    public class DebugProjectSelectorTests : E2ETestBase
    {
        [Fact]
        public async Task Debug_ProjectSelector_ShowDropdownContents()
        {
            // Navigate to home page
            await Page.GotoAsync($"{BaseUrl}/");
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            
            Console.WriteLine("=== Debugging Project Selector ===");
            
            // Check if project selector exists
            var selectorContainer = Page.Locator(".project-selector-container");
            var containerExists = await selectorContainer.CountAsync() > 0;
            Console.WriteLine($"Project selector container exists: {containerExists}");
            
            if (containerExists)
            {
                var containerHTML = await selectorContainer.InnerHTMLAsync();
                Console.WriteLine($"Container HTML: {containerHTML}");
            }
            
            // Look for any button in the project selector
            var allButtons = await Page.Locator(".project-selector-container button").AllAsync();
            Console.WriteLine($"Found {allButtons.Count} buttons in project selector");
            
            foreach (var button in allButtons)
            {
                var isVisible = await button.IsVisibleAsync();
                var text = await button.TextContentAsync();
                var className = await button.GetAttributeAsync("class");
                Console.WriteLine($"Button: visible={isVisible}, text='{text}', class='{className}'");
                
                if (isVisible)
                {
                    // Click the button to open dropdown
                    Console.WriteLine("Clicking project selector button...");
                    await button.ClickAsync();
                    await Page.WaitForTimeoutAsync(2000);
                    
                    // Check for dropdown
                    var dropdown = Page.Locator(".dropdown-menu");
                    var dropdownVisible = await dropdown.IsVisibleAsync();
                    Console.WriteLine($"Dropdown visible: {dropdownVisible}");
                    
                    if (dropdownVisible)
                    {
                        var dropdownItems = await dropdown.Locator(".dropdown-item").AllAsync();
                        Console.WriteLine($"Dropdown items found: {dropdownItems.Count}");
                        
                        foreach (var item in dropdownItems)
                        {
                            var itemText = await item.TextContentAsync();
                            var itemVisible = await item.IsVisibleAsync();
                            Console.WriteLine($"Item: visible={itemVisible}, text='{itemText?.Trim()}'");
                        }
                    }
                    break;
                }
            }
            
            // Check browser console for errors
            Page.Console += (_, e) => Console.WriteLine($"BROWSER: {e.Type}: {e.Text}");
            
            // Take a screenshot for debugging
            await Page.ScreenshotAsync(new() { Path = "debug_project_selector.png", FullPage = true });
            Console.WriteLine("Screenshot saved as debug_project_selector.png");
        }
    }
}
