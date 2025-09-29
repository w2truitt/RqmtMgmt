using Microsoft.Playwright;
using System;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        // Install browsers if needed
        var exitCode = Microsoft.Playwright.Program.Main(new[] { "install" });
        if (exitCode != 0)
        {
            Console.WriteLine("Failed to install Playwright browsers");
            return;
        }

        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true,
            Args = new[] { "--no-sandbox", "--disable-setuid-sandbox" }
        });

        var context = await browser.NewContextAsync(new BrowserNewContextOptions
        {
            IgnoreHTTPSErrors = true
        });

        var page = await context.NewPageAsync();

        try
        {
            // Navigate to the projects page
            await page.GotoAsync("https://rqmtmgmt.local/projects");

            // Wait for the page to load
            await page.WaitForSelectorAsync("table, .project-card, [data-testid*='project']", new PageWaitForSelectorOptions
            {
                Timeout = 10000
            });

            // Try different selectors to find project names
            var projectSelectors = new[]
            {
                "table tbody tr:first-child td:first-child a", // Table row with link
                "table tbody tr:first-child td:nth-child(2)", // Table row second column
                ".project-card:first-child h3, .project-card:first-child .project-name", // Project card
                "[data-testid*='project']:first-child [data-testid*='name'], [data-testid*='project']:first-child a", // Data testid
                "a[href*='project']:first-child", // Link with project in href
                ".project-item:first-child, .project-link:first-child" // Generic project item
            };

            string firstProjectName = null;
            foreach (var selector in projectSelectors)
            {
                try
                {
                    var element = await page.QuerySelectorAsync(selector);
                    if (element != null)
                    {
                        var text = await element.TextContentAsync();
                        if (!string.IsNullOrWhiteSpace(text))
                        {
                            firstProjectName = text.Trim();
                            break;
                        }
                    }
                }
                catch
                {
                    // Continue to next selector
                }
            }

            if (firstProjectName != null)
            {
                Console.WriteLine($"First project name: {firstProjectName}");
            }
            else
            {
                // Try to get all text content from the page to debug
                var bodyText = await page.TextContentAsync("body");
                Console.WriteLine("Page content (first 500 chars):");
                Console.WriteLine(bodyText?.Substring(0, Math.Min(500, bodyText.Length)));
                Console.WriteLine("Could not find project name with available selectors");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
        finally
        {
            await browser.CloseAsync();
        }
    }
}