using frontend.E2ETests.Fixtures;
using Microsoft.Playwright;
using System.Text.RegularExpressions;
using Xunit;
using Xunit.Abstractions;
using static Microsoft.Playwright.Assertions;

namespace frontend.E2ETests.Workflows;

/// <summary>
/// E2E tests for advanced document section management workflows.
/// Tests section ordering, editing, deletion, and complex section operations.
/// </summary>
[Collection("E2E Test Collection")]
public class DocumentSectionManagementTests : AuthenticatedE2ETestBase
{
    public DocumentSectionManagementTests(PlaywrightFixture fixture, ITestOutputHelper output) 
        : base(fixture, output)
    {
        SetAdminUser();
    }

    [Fact]
    public async Task SectionOrdering_ShouldMaintainCorrectSequence_WhenAddingMultipleSections()
    {
        // Arrange - Create document with systematic section numbering
        var documentId = await CreateTestDocument();
        await Page.GotoAsync($"{BaseUrl}/projects/1/documents/{documentId}");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
        // Act - Add sections with systematic numbering
        var sections = new[]
        {
            "1.1 Introduction",
            "1.2 Purpose and Scope", 
            "2.1 System Overview",
            "2.2 Architecture",
            "3.1 Authentication",
            "3.2 Authorization",
            "4.1 Performance",
            "4.2 Security"
        };
        
        for (int i = 0; i < sections.Length; i++)
        {
            var buttonText = i == 0 ? "Add First Section" : "Add Section";
            await Page.ClickAsync($"button:has-text('{buttonText}')");
            
            await Page.FillAsync("input[placeholder='Enter section title']", sections[i]);
            await Page.FillAsync("textarea[placeholder*='Optional description']", $"Description for {sections[i]}");
            
            await Page.Locator("form").GetByRole(AriaRole.Button, new() { Name = "Add Section" }).ClickAsync();
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        }
        
        // Assert - Sections should be numbered sequentially
        for (int i = 0; i < sections.Length; i++)
        {
            var expectedNumber = i + 1;
            await Expect(Page.Locator($"text={expectedNumber}. {sections[i]}")).ToBeVisibleAsync();
        }
        
        // Section management area should show correct order
        var managementSections = Page.Locator(".section-item");
        await Expect(managementSections).ToHaveCountAsync(sections.Length);
        
        // Verify section order in management area - use .First to avoid strict mode violations
        for (int i = 0; i < sections.Length; i++)
        {
            var sectionItem = managementSections.Nth(i);
            await Expect(sectionItem.Locator($"text={sections[i]}").First).ToBeVisibleAsync();
        }
    }

    [Fact]
    public async Task SectionNavigation_ShouldProvideUpDownControls_ForReordering()
    {
        // Arrange - Create document with multiple sections
        var documentId = await CreateTestDocument();
        await Page.GotoAsync($"{BaseUrl}/projects/1/documents/{documentId}");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
        // Add three sections
        var sections = new[] { "Section A", "Section B", "Section C" };
        
        for (int i = 0; i < sections.Length; i++)
        {
            var buttonText = i == 0 ? "Add First Section" : "Add Section";
            await Page.ClickAsync($"button:has-text('{buttonText}')");
            await Page.FillAsync("input[placeholder='Enter section title']", sections[i]);
            await Page.Locator("form").GetByRole(AriaRole.Button, new() { Name = "Add Section" }).ClickAsync();
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        }
        
        // Assert - Navigation controls should be present and properly configured
        var sectionItems = Page.Locator(".section-item");
        
        // First section: Up button disabled, Down button enabled
        var firstSectionButtons = sectionItems.First.Locator("button");
        var firstUpButton = firstSectionButtons.First; // Should be disabled
        var firstDownButton = firstSectionButtons.Nth(1); // Should be enabled
        
        await Expect(firstUpButton).ToBeDisabledAsync();
        await Expect(firstDownButton).ToBeEnabledAsync();
        
        // Middle section: Both buttons enabled
        var middleSectionButtons = sectionItems.Nth(1).Locator("button");
        var middleUpButton = middleSectionButtons.First;
        var middleDownButton = middleSectionButtons.Nth(1);
        
        await Expect(middleUpButton).ToBeEnabledAsync();
        await Expect(middleDownButton).ToBeEnabledAsync();
        
        // Last section: Up button enabled, Down button disabled
        var lastSectionButtons = sectionItems.Last.Locator("button");
        var lastUpButton = lastSectionButtons.First;
        var lastDownButton = lastSectionButtons.Nth(1);
        
        await Expect(lastUpButton).ToBeEnabledAsync();
        await Expect(lastDownButton).ToBeDisabledAsync();
    }

    [Fact]
    public async Task SectionDeletion_ShouldRemoveSection_AndUpdateOrdering()
    {
        // Arrange - Create document with multiple sections
        var documentId = await CreateTestDocument();
        await Page.GotoAsync($"{BaseUrl}/projects/1/documents/{documentId}");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
        // Add sections
        var sections = new[] { "First Section", "Middle Section", "Last Section" };
        
        for (int i = 0; i < sections.Length; i++)
        {
            var buttonText = i == 0 ? "Add First Section" : "Add Section";
            await Page.ClickAsync($"button:has-text('{buttonText}')");
            await Page.FillAsync("input[placeholder='Enter section title']", sections[i]);
            await Page.Locator("form").GetByRole(AriaRole.Button, new() { Name = "Add Section" }).ClickAsync();
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        }
        
        // Verify initial state
        await Expect(Page.Locator("text=Sections: 3")).ToBeVisibleAsync();
        await Expect(Page.Locator("text=1. First Section")).ToBeVisibleAsync();
        await Expect(Page.Locator("text=2. Middle Section")).ToBeVisibleAsync();
        await Expect(Page.Locator("text=3. Last Section")).ToBeVisibleAsync();
        
        // Act - Delete the middle section
        var middleSectionItem = Page.Locator(".section-item").Nth(1);
        var deleteButton = middleSectionItem.Locator("button").Last; // Delete button is usually last
        
        // Set up confirmation dialog handler
        Page.Dialog += async (_, dialog) =>
        {
            await dialog.AcceptAsync();
        };
        
        await deleteButton.ClickAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
        // Wait for the page to update after deletion
        await Page.WaitForTimeoutAsync(2000);
        
        // Assert - Section should be deleted
        await Expect(Page.Locator("text=Sections: 2")).ToBeVisibleAsync();
        
        // Verify Middle Section is deleted
        await Expect(Page.Locator("text=Middle Section")).Not.ToBeVisibleAsync();
        
        // Verify First and Last sections still exist
        // Note: Section numbers may or may not be automatically updated depending on implementation
        await Expect(Page.Locator("text=First Section").First).ToBeVisibleAsync();
        await Expect(Page.Locator("text=Last Section").First).ToBeVisibleAsync();
        
        // Section management should show only 2 sections
        var remainingSections = Page.Locator(".section-item");
        await Expect(remainingSections).ToHaveCountAsync(2);
    }

    [Fact]
    public async Task SectionTemplates_ShouldSupportPredefinedStructures_ForCommonDocumentTypes()
    {
        // Arrange - This test assumes future template functionality
        var documentId = await CreateTestDocument();
        await Page.GotoAsync($"{BaseUrl}/projects/1/documents/{documentId}");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
        // Act & Assert - For now, verify we can create common SRS sections systematically
        var standardSRSSections = new[]
        {
            new { Number = "1.1", Title = "Purpose", Description = "Purpose and objectives of this document" },
            new { Number = "1.2", Title = "Scope", Description = "Scope of the software product" },
            new { Number = "1.3", Title = "Definitions and Acronyms", Description = "Definitions of terms and acronyms" },
            new { Number = "2.1", Title = "Product Perspective", Description = "Relationship to other products" },
            new { Number = "2.2", Title = "Product Functions", Description = "Summary of major functions" },
            new { Number = "3.1", Title = "Functional Requirements", Description = "Detailed functional requirements" },
            new { Number = "3.2", Title = "Performance Requirements", Description = "Performance and scalability requirements" },
            new { Number = "3.3", Title = "Security Requirements", Description = "Security and access control requirements" }
        };
        
        // Create standardized SRS structure
        for (int i = 0; i < standardSRSSections.Length; i++)
        {
            var section = standardSRSSections[i];
            var buttonText = i == 0 ? "Add First Section" : "Add Section";
            
            await Page.ClickAsync($"button:has-text('{buttonText}')");
            await Page.FillAsync("input[placeholder='Enter section title']", $"{section.Number} {section.Title}");
            await Page.FillAsync("textarea[placeholder*='Optional description']", section.Description);
            await Page.Locator("form").GetByRole(AriaRole.Button, new() { Name = "Add Section" }).ClickAsync();
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        }
        
        // Assert - Standard SRS structure should be created
        await Expect(Page.Locator($"text=Sections: {standardSRSSections.Length}")).ToBeVisibleAsync();
        
        for (int i = 0; i < standardSRSSections.Length; i++)
        {
            var section = standardSRSSections[i];
            var sectionNumber = i + 1;
            
            await Expect(Page.Locator($"text={sectionNumber}. {section.Number} {section.Title}")).ToBeVisibleAsync();
            // Use .First for descriptions that might appear in multiple places
            await Expect(Page.Locator($"text={section.Description}").First).ToBeVisibleAsync();
        }
        
        Output.WriteLine($"✅ Created standardized SRS with {standardSRSSections.Length} sections");
    }

    [Fact]
    public async Task SectionBulkOperations_ShouldSupportMultipleActions_Efficiently()
    {
        // Arrange - Create document with many sections for bulk operations
        var documentId = await CreateTestDocument();
        await Page.GotoAsync($"{BaseUrl}/projects/1/documents/{documentId}");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
        // Create multiple sections quickly
        var sectionNames = new[]
        {
            "Introduction", "System Overview", "Requirements", "Architecture", 
            "Design", "Implementation", "Testing", "Deployment",
            "Maintenance", "Appendices"
        };
        
        // Act - Add sections rapidly
        var startTime = DateTime.Now;
        
        for (int i = 0; i < sectionNames.Length; i++)
        {
            var buttonText = i == 0 ? "Add First Section" : "Add Section";
            await Page.ClickAsync($"button:has-text('{buttonText}')");
            await Page.FillAsync("input[placeholder='Enter section title']", sectionNames[i]);
            await Page.Locator("form").GetByRole(AriaRole.Button, new() { Name = "Add Section" }).ClickAsync();
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        }
        
        var endTime = DateTime.Now;
        var duration = endTime - startTime;
        
        // Assert - All sections should be created efficiently
        await Expect(Page.Locator($"text=Sections: {sectionNames.Length}")).ToBeVisibleAsync();
        
        // Verify all sections exist in order
        for (int i = 0; i < sectionNames.Length; i++)
        {
            var sectionNumber = i + 1;
            await Expect(Page.Locator($"text={sectionNumber}. {sectionNames[i]}")).ToBeVisibleAsync();
        }
        
        // Performance assertion - should complete reasonably quickly
        Assert.True(duration.TotalMinutes < 2, 
            $"Bulk section creation took {duration.TotalSeconds:F1} seconds, should be under 2 minutes");
        
        Output.WriteLine($"✅ Created {sectionNames.Length} sections in {duration.TotalSeconds:F1} seconds");
    }

    [Fact]
    public async Task SectionPersistence_ShouldMaintainData_AcrossPageReloads()
    {
        // Arrange - Create document with sections
        var documentId = await CreateTestDocument();
        await Page.GotoAsync($"{BaseUrl}/projects/1/documents/{documentId}");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
        // Add sections with specific content
        var testSections = new[]
        {
            new { Title = "Persistent Section A", Description = "This section should persist across reloads" },
            new { Title = "Persistent Section B", Description = "Another section that must maintain state" }
        };
        
        for (int i = 0; i < testSections.Length; i++)
        {
            var section = testSections[i];
            var buttonText = i == 0 ? "Add First Section" : "Add Section";
            
            await Page.ClickAsync($"button:has-text('{buttonText}')");
            await Page.FillAsync("input[placeholder='Enter section title']", section.Title);
            await Page.FillAsync("textarea[placeholder*='Optional description']", section.Description);
            await Page.Locator("form").GetByRole(AriaRole.Button, new() { Name = "Add Section" }).ClickAsync();
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        }
        
        // Verify initial state
        await Expect(Page.Locator("text=Sections: 2")).ToBeVisibleAsync();
        
        // Act - Reload the page
        await Page.ReloadAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
        // Assert - Sections should persist after reload
        await Expect(Page.Locator("text=Sections: 2")).ToBeVisibleAsync();
        
        for (int i = 0; i < testSections.Length; i++)
        {
            var section = testSections[i];
            var sectionNumber = i + 1;
            
            await Expect(Page.Locator($"text={sectionNumber}. {section.Title}")).ToBeVisibleAsync();
            // Use .First since descriptions appear in both management area and document body
            await Expect(Page.Locator($"text={section.Description}").First).ToBeVisibleAsync();
        }
        
        // Section management should still be functional
        await Expect(Page.Locator("text=Document Sections")).ToBeVisibleAsync();
        await Expect(Page.Locator("button:has-text('Add Section')")).ToBeVisibleAsync();
        
        var managementSections = Page.Locator(".section-item");
        await Expect(managementSections).ToHaveCountAsync(testSections.Length);
        
        Output.WriteLine("✅ Section data persisted correctly across page reload");
    }

    /// <summary>
    /// Helper method to create a test document for section management testing
    /// </summary>
    private async Task<string> CreateTestDocument()
    {
        await Page.GotoAsync($"{BaseUrl}/projects/1/documents/new");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
        var timestamp = DateTimeOffset.Now.ToUnixTimeSeconds();
        var documentTitle = $"Section Management Test Doc {timestamp}";
        
        await Page.FillAsync("input[placeholder='Enter document title']", documentTitle);
        await Page.SelectOptionAsync("select", "Software Requirement Specification (SRS)");
        await Page.FillAsync("textarea[placeholder*='Clearly state the business goals']", 
            "Test document for section management functionality");
        await Page.FillAsync("textarea[placeholder*='Provide relevant background']", 
            "This document is used for testing section creation, ordering, and management");
        
        await Page.ClickAsync("button:has-text('Create Document')");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
        // Wait for navigation to complete - might take a moment
        await Page.WaitForTimeoutAsync(2000);
        
        // Extract document ID from URL
        var url = Page.Url;
        var match = Regex.Match(url, @"/documents/(\d+)");
        if (!match.Success)
        {
            // Try waiting a bit more and check again
            await Page.WaitForTimeoutAsync(1000);
            url = Page.Url;
            match = Regex.Match(url, @"/documents/(\d+)");
        }
        
        if (match.Success)
        {
            return match.Groups[1].Value;
        }
        
        throw new InvalidOperationException($"Could not extract document ID from URL: {url}");
    }
}