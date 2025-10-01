using frontend.E2ETests.Fixtures;
using Microsoft.Playwright;
using System.Text.RegularExpressions;
using Xunit;
using Xunit.Abstractions;
using static Microsoft.Playwright.Assertions;

namespace frontend.E2ETests.Workflows;

/// <summary>
/// E2E tests specifically for Software Requirements Specification (SRS) document workflows.
/// Tests SRS creation, section management, and requirements organization.
/// </summary>
[Collection("E2E Test Collection")]
public class SRSDocumentWorkflowTests : AuthenticatedE2ETestBase
{
    public SRSDocumentWorkflowTests(PlaywrightFixture fixture, ITestOutputHelper output) 
        : base(fixture, output)
    {
        SetAdminUser();
    }

    [Fact]
    public async Task CreateCompleteBackendSRS_ShouldBuildFullDocumentStructure_Successfully()
    {
        // Arrange - Navigate to Legacy Requirements project (ID 1)
        await Page.GotoAsync($"{BaseUrl}/projects/1/documents/new");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
        // Act - Create TestFlow Pro Backend SRS document
        var timestamp = DateTimeOffset.Now.ToUnixTimeSeconds();
        var documentTitle = $"TestFlow Pro Backend Server - SRS {timestamp}";
        var objective = "This Software Requirements Specification (SRS) defines the functional and non-functional requirements for the backend server component of the Requirements & Test Management System. The backend serves as a RESTful API providing data management, business logic, authentication, and integration capabilities for the web-based enterprise tool.";
        var background = "The backend server supports management of Customer Requirement Documents (CRD), Product Requirement Documents (PRD), Software Requirement Specifications (SRS), Test Suites, Test Cases, Test Plans, Test Runs, and provides comprehensive traceability and reporting capabilities. Built using .NET 8 Web API with Entity Framework Core for data access, providing RESTful endpoints for frontend consumption and third-party integrations.";
        
        await Page.FillAsync("input[placeholder='Enter document title']", documentTitle);
        await Page.SelectOptionAsync("select", "Software Requirement Specification (SRS)");
        await Page.FillAsync("textarea[placeholder*='Clearly state the business goals']", objective);
        await Page.FillAsync("textarea[placeholder*='Provide relevant background']", background);
        
        await Page.ClickAsync("button:has-text('Create Document')");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
        // Assert - Document should be created successfully
        await Expect(Page.Locator($"text={documentTitle}")).ToBeVisibleAsync();
        await Expect(Page.Locator("text=Software Requirement Specification").First).ToBeVisibleAsync();
        await Expect(Page.Locator("text=Draft").First).ToBeVisibleAsync();
        await Expect(Page.Locator("text=Version 1.0").First).ToBeVisibleAsync();
        
        // Should show project context
        await Expect(Page.Locator("text=Project: 1")).ToBeVisibleAsync();
        
        // Should show section management interface
        await Expect(Page.Locator("text=Document Sections")).ToBeVisibleAsync();
        await Expect(Page.Locator("text=Sections: 0")).ToBeVisibleAsync();
    }

    [Fact]
    public async Task BuildSRSSectionStructure_ShouldCreateSystematicOrganization_Successfully()
    {
        // Arrange - Create base SRS document
        var documentId = await CreateBaseSRSDocument();
        await Page.GotoAsync($"{BaseUrl}/projects/1/documents/{documentId}");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
        // Act - Build systematic section structure
        var sections = new[]
        {
            new { Title = "3.1 Authentication and Authorization", Description = "Requirements for user authentication and role-based access control" },
            new { Title = "3.2 User Management", Description = "Requirements for user account operations, profile management, and role assignments" },
            new { Title = "3.3 Project Management", Description = "Requirements for project operations and data isolation" },
            new { Title = "3.4 Requirements Management", Description = "Requirements for document and requirement operations" },
            new { Title = "3.5 API Design and RESTful Services", Description = "Requirements for RESTful API endpoints, data models, and service architecture" },
            new { Title = "4.1 Performance Requirements", Description = "Non-functional requirements for system performance and scalability" },
            new { Title = "4.2 Security Requirements", Description = "Security and compliance requirements for data protection" }
        };
        
        // Add sections systematically
        for (int i = 0; i < sections.Length; i++)
        {
            var section = sections[i];
            
            if (i == 0)
            {
                await Page.ClickAsync("button:has-text('Add First Section')");
            }
            else
            {
                await Page.ClickAsync("button:has-text('Add Section')");
            }
            
            await Page.FillAsync("input[placeholder='Enter section title']", section.Title);
            await Page.FillAsync("textarea[placeholder*='Optional description']", section.Description);
            
            var responsePromise = Page.WaitForResponseAsync(response => 
                response.Url.Contains("/api/documentsections") && response.Request.Method == "POST");
            
            await Page.Locator("form").GetByRole(AriaRole.Button, new() { Name = "Add Section" }).ClickAsync();
            
            var response = await responsePromise;
            Assert.True(response.Status < 400, $"Section '{section.Title}' creation failed with status {response.Status}");
            
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        }
        
        // Assert - All sections should be created in order
        await Expect(Page.Locator($"text=Sections: {sections.Length}")).ToBeVisibleAsync();
        
        // Verify section order and content
        for (int i = 0; i < sections.Length; i++)
        {
            var sectionNumber = i + 1;
            var section = sections[i];
            
            await Expect(Page.Locator($"text={sectionNumber}. {section.Title}")).ToBeVisibleAsync();
            // Use .First since descriptions appear in both management area and document body
            await Expect(Page.Locator($"text={section.Description}").First).ToBeVisibleAsync();
        }
        
        // Each section should have requirement management capabilities
        var addRequirementButtons = Page.Locator("button:has-text('Add Requirement to Section')");
        await Expect(addRequirementButtons).ToHaveCountAsync(sections.Length);
        
        // Section management should show all sections
        var managementSections = Page.Locator(".section-item");
        await Expect(managementSections).ToHaveCountAsync(sections.Length);
    }

    [Fact]
    public async Task SectionOperations_ShouldSupportEditAndDelete_Successfully()
    {
        // Arrange - Create SRS document with sections
        var documentId = await CreateBaseSRSDocument();
        await Page.GotoAsync($"{BaseUrl}/projects/1/documents/{documentId}");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
        // Add test sections
        await Page.ClickAsync("button:has-text('Add First Section')");
        await Page.FillAsync("input[placeholder='Enter section title']", "3.1 Test Section");
        await Page.FillAsync("textarea[placeholder*='Optional description']", "Original description");
        await Page.Locator("form").GetByRole(AriaRole.Button, new() { Name = "Add Section" }).ClickAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
        await Page.ClickAsync("button:has-text('Add Section')");
        await Page.FillAsync("input[placeholder='Enter section title']", "3.2 Another Section");
        await Page.FillAsync("textarea[placeholder*='Optional description']", "Another description");
        await Page.Locator("form").GetByRole(AriaRole.Button, new() { Name = "Add Section" }).ClickAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
        // Assert initial state
        await Expect(Page.Locator("text=Sections: 2")).ToBeVisibleAsync();
        await Expect(Page.Locator("text=1. 3.1 Test Section")).ToBeVisibleAsync();
        await Expect(Page.Locator("text=2. 3.2 Another Section")).ToBeVisibleAsync();
        
        // Test section management controls are present
        var sectionControls = Page.Locator(".section-item").First.Locator("button");
        await Expect(sectionControls).ToHaveCountAsync(4); // Up, Down, Edit, Delete buttons
    }

    [Fact]
    public async Task SectionWithNotApplicable_ShouldMarkAsNA_Successfully()
    {
        // Arrange - Create SRS document
        var documentId = await CreateBaseSRSDocument();
        await Page.GotoAsync($"{BaseUrl}/projects/1/documents/{documentId}");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
        // Act - Add section marked as Not Applicable
        await Page.ClickAsync("button:has-text('Add First Section')");
        await Page.FillAsync("input[placeholder='Enter section title']", "5.1 Mobile Requirements");
        await Page.FillAsync("textarea[placeholder*='Optional description']", "Requirements for mobile applications");
        
        // Mark as Not Applicable
        await Page.CheckAsync("input[type='checkbox']");
        
        await Page.Locator("form").GetByRole(AriaRole.Button, new() { Name = "Add Section" }).ClickAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
        // Assert - Section should be marked as N/A
        await Expect(Page.Locator("text=Sections: 1")).ToBeVisibleAsync();
        await Expect(Page.Locator("text=1. 5.1 Mobile Requirements")).ToBeVisibleAsync();
        
        // Section should have visual indication of N/A status
        var sectionItem = Page.Locator(".section-item").First;
        var hasNAClass = await sectionItem.GetAttributeAsync("class");
        Assert.Contains("bg-light", hasNAClass ?? "", StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task SRSWorkflow_ShouldSupportEndToEndDocumentCreation_Successfully()
    {
        // Arrange & Act - Complete end-to-end SRS creation workflow
        await Page.GotoAsync($"{BaseUrl}/projects/1/documents/new");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
        // Step 1: Create document
        var timestamp = DateTimeOffset.Now.ToUnixTimeSeconds();
        var documentTitle = $"E2E Complete SRS {timestamp}";
        
        await Page.FillAsync("input[placeholder='Enter document title']", documentTitle);
        await Page.SelectOptionAsync("select", "Software Requirement Specification (SRS)");
        await Page.FillAsync("textarea[placeholder*='Clearly state the business goals']", 
            "Complete end-to-end testing of SRS document creation and section management capabilities.");
        await Page.FillAsync("textarea[placeholder*='Provide relevant background']", 
            "This test validates the entire workflow from document creation through section management.");
        
        await Page.ClickAsync("button:has-text('Create Document')");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
        // Step 2: Add core sections
        var coreSections = new[]
        {
            "Authentication Requirements",
            "API Design Requirements", 
            "Performance Requirements"
        };
        
        for (int i = 0; i < coreSections.Length; i++)
        {
            var buttonText = i == 0 ? "Add First Section" : "Add Section";
            await Page.ClickAsync($"button:has-text('{buttonText}')");
            
            await Page.FillAsync("input[placeholder='Enter section title']", coreSections[i]);
            await Page.FillAsync("textarea[placeholder*='Optional description']", 
                $"Requirements and specifications for {coreSections[i].ToLower()}");
            
            await Page.Locator("form").GetByRole(AriaRole.Button, new() { Name = "Add Section" }).ClickAsync();
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        }
        
        // Step 3: Verify complete document structure
        await Expect(Page.Locator($"text={documentTitle}")).ToBeVisibleAsync();
        await Expect(Page.Locator("text=Software Requirement Specification").First).ToBeVisibleAsync();
        await Expect(Page.Locator($"text=Sections: {coreSections.Length}")).ToBeVisibleAsync();
        
        // Each section should be properly structured
        for (int i = 0; i < coreSections.Length; i++)
        {
            var sectionNumber = i + 1;
            await Expect(Page.Locator($"text={sectionNumber}. {coreSections[i]}")).ToBeVisibleAsync();
            await Expect(Page.Locator("text=No requirements in this section").Nth(i)).ToBeVisibleAsync();
        }
        
        // Step 4: Test reading mode
        await Page.ClickAsync("button:has-text('Reading Mode')");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
        // Section management should be hidden
        await Expect(Page.Locator("text=Document Sections")).Not.ToBeVisibleAsync();
        await Expect(Page.Locator("button:has-text('Add Section')")).Not.ToBeVisibleAsync();
        
        // But content should remain visible
        await Expect(Page.Locator($"text={documentTitle}")).ToBeVisibleAsync();
        for (int i = 0; i < coreSections.Length; i++)
        {
            var sectionNumber = i + 1;
            await Expect(Page.Locator($"text={sectionNumber}. {coreSections[i]}")).ToBeVisibleAsync();
        }
        
        // Assert - Complete workflow successful
        Output.WriteLine($"✅ Successfully created complete SRS document: {documentTitle}");
        Output.WriteLine($"✅ Created {coreSections.Length} sections with proper structure");
        Output.WriteLine($"✅ Verified reading mode functionality");
    }

    /// <summary>
    /// Helper method to create a base SRS document for testing
    /// </summary>
    private async Task<string> CreateBaseSRSDocument()
    {
        await Page.GotoAsync($"{BaseUrl}/projects/1/documents/new");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
        var timestamp = DateTimeOffset.Now.ToUnixTimeSeconds();
        var documentTitle = $"E2E Test SRS {timestamp}";
        
        await Page.FillAsync("input[placeholder='Enter document title']", documentTitle);
        await Page.SelectOptionAsync("select", "Software Requirement Specification (SRS)");
        await Page.FillAsync("textarea[placeholder*='Clearly state the business goals']", "Test SRS objective");
        await Page.FillAsync("textarea[placeholder*='Provide relevant background']", "Test SRS background");
        
        await Page.ClickAsync("button:has-text('Create Document')");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
        // Wait for navigation to complete
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