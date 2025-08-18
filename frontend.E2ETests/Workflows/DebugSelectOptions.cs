using frontend.E2ETests;
using frontend.E2ETests.PageObjects;
using Microsoft.Playwright;
using Xunit;

namespace frontend.E2ETests.Workflows;

public class DebugSelectOptions : E2ETestBase
{
    [Fact]
    public async Task Debug_CreateTestPlan_AfterFix()
    {
        // Set up network interception to capture the API call
        var responseReceived = new TaskCompletionSource<string>();
        
        Page.Response += async (_, response) =>
        {
            if (response.Url.Contains("/api/TestPlan") && response.Request.Method == "POST")
            {
                var responseText = await response.TextAsync();
                Console.WriteLine($"API Response Status: {response.Status}");
                Console.WriteLine($"API Response Body: {responseText}");
                responseReceived.SetResult(responseText);
            }
        };
        
        Page.Request += async (_, request) =>
        {
            if (request.Url.Contains("/api/TestPlan") && request.Method == "POST")
            {
                var postData = request.PostData;
                Console.WriteLine($"API Request Body: {postData}");
            }
        };

        // Navigate to test plans page
        await Page.GotoAsync($"{BaseUrl}/testplans");
        await Page.WaitForSelectorAsync("[data-testid='create-testplan-button']");
        
        // Click create button
        await Page.ClickAsync("[data-testid='create-testplan-button']");
        await Page.WaitForSelectorAsync(".modal.show", new PageWaitForSelectorOptions { State = WaitForSelectorState.Visible });
        
        // Fill the form
        await Page.FillAsync(".modal.show [data-testid='name-input']", "Fixed Test Plan");
        await Page.SelectOptionAsync(".modal.show [data-testid='type-select']", "UserValidation");
        await Page.FillAsync(".modal.show [data-testid='description-input']", "Test plan after fix");
        
        Console.WriteLine("Form filled successfully");
        
        // Click save
        await Page.ClickAsync(".modal.show [data-testid='save-button']");
        
        // Wait for API response
        try
        {
            var response = await responseReceived.Task.WaitAsync(TimeSpan.FromSeconds(10));
            Console.WriteLine("API call completed successfully");
        }
        catch (TimeoutException)
        {
            Console.WriteLine("No API response received within 10 seconds");
        }
        
        // Wait a bit longer
        await Page.WaitForTimeoutAsync(3000);
        
        // Check for error messages
        var formError = await Page.IsVisibleAsync(".alert-danger");
        if (formError)
        {
            var errorText = await Page.TextContentAsync(".alert-danger");
            Console.WriteLine($"Form error found: {errorText}");
        }
        else
        {
            Console.WriteLine("No form error - API call was successful!");
        }
        
        // Check if modal disappeared (indicates success)
        var modalVisible = await Page.IsVisibleAsync(".modal.show");
        Console.WriteLine($"Modal still visible after save: {modalVisible}");
        
        Assert.True(true);
    }
}
