using Microsoft.Playwright;
using Xunit;
using System.Net.Http;

namespace frontend.E2ETests.Debug;

[Collection("E2E Tests")]
public class ApiDebugTest : E2ETestBase
{
    [Fact]
    public async Task DebugApiCall()
    {
        // Set up console message capture
        var consoleMessages = new List<string>();
        Page.Console += (_, msg) => {
            consoleMessages.Add($"{msg.Type}: {msg.Text}");
        };
        
        // Navigate to the projects page first
        await Page.GotoAsync($"{BaseUrl}/projects");
        
        // Wait for the page to load and any authentication to happen
        await Page.WaitForTimeoutAsync(3000);
        
        // Now make a direct API call using the browser context
        var apiResponse = await Page.EvaluateAsync<string>(@"
            async () => {
                try {
                    console.log('Making API call to /api/projects');
                    const response = await fetch('/api/projects', {
                        method: 'GET',
                        headers: {
                            'Content-Type': 'application/json'
                        }
                    });
                    console.log('Response status:', response.status);
                    const text = await response.text();
                    console.log('Response text:', text.substring(0, 200));
                    return `Status: ${response.status}, Body: ${text.substring(0, 500)}`;
                } catch (error) {
                    console.error('API call error:', error);
                    return `Error: ${error.message}`;
                }
            }
        ");
        
        Console.WriteLine($"API Response: {apiResponse}");
        
        // Also try getting a specific project
        var projectApiResponse = await Page.EvaluateAsync<string>(@"
            async () => {
                try {
                    const response = await fetch('/api/projects/1');
                    const status = response.status;
                    const text = await response.text();
                    return `Status: ${status}, Body: ${text.substring(0, 500)}`;
                } catch (error) {
                    return `Error: ${error.message}`;
                }
            }
        ");
        
        Console.WriteLine($"Project 1 API Response: {projectApiResponse}");
        
        // Check what cookies are set
        var cookies = await Page.Context.CookiesAsync();
        Console.WriteLine($"Found {cookies.Count} cookies:");
        foreach (var cookie in cookies)
        {
            Console.WriteLine($"  {cookie.Name}: {cookie.Value.Substring(0, Math.Min(50, cookie.Value.Length))}...");
        }
        
        // Print console messages
        Console.WriteLine($"Console messages ({consoleMessages.Count}):");
        foreach (var msg in consoleMessages)
        {
            Console.WriteLine($"  {msg}");
        }
        
        Assert.True(true);
    }
}
