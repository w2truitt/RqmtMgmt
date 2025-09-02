using frontend.E2ETests.PageObjects;
using Microsoft.Playwright;
using Xunit;
using Xunit.Abstractions;

namespace frontend.E2ETests.Workflows
{
    public class DebugProjectSelectorTests : AuthenticatedE2ETestBase
    {
        public DebugProjectSelectorTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task Debug_ProjectSelector_ShowDropdownContents()
        {
            // Arrange - Login as developer for debugging access
            var loginSuccess = await LoginAsDeveloperAsync();
            Assert.True(loginSuccess, "Failed to login as developer");
            
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
                var buttonText = await button.TextContentAsync();
                var buttonClass = await button.GetAttributeAsync("class");
                Console.WriteLine($"Button: '{buttonText}' (class: {buttonClass})");
            }
            
            // Try to click the first button if it exists
            if (allButtons.Count > 0)
            {
                await allButtons[0].ClickAsync();
                await Task.Delay(1000);
                
                // Check for dropdown menu
                var dropdownMenu = Page.Locator(".dropdown-menu");
                var dropdownExists = await dropdownMenu.CountAsync() > 0;
                Console.WriteLine($"Dropdown menu appeared: {dropdownExists}");
                
                if (dropdownExists)
                {
                    var dropdownHTML = await dropdownMenu.InnerHTMLAsync();
                    Console.WriteLine($"Dropdown HTML: {dropdownHTML}");
                }
            }
            
            // Assert that debug completed
            Assert.True(true, "Debug test completed - check console output");
        }
    }
}