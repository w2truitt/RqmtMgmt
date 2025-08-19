using Bunit;
using frontend.Layout;
using frontend.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using RqmtMgmtShared;
using Xunit;
using AngleSharp.Dom;

namespace frontend.ComponentTests.Components.SharedTests;

/// <summary>
/// Tests for the NavMenu component
/// </summary>
public class NavMenuTests : ComponentTestBase
{
    [Fact]
    public void NavMenu_RendersCorrectly_WithAllNavigationLinks_NoProjectContext()
    {
        // Arrange
        var mockProjectService = GetMockService<IProjectContextService>();
        mockProjectService.Setup(s => s.IsInProjectContext).Returns(false);
        mockProjectService.Setup(s => s.CurrentProject).Returns((ProjectDto?)null);

        // Act
        var component = RenderComponent<NavMenu>();
        
        // Assert
        Assert.Contains("TestFlow Pro", component.Markup); // Brand name
        
        // Check all navigation links are present (excluding project selector which adds one more)
        var navLinks = component.FindAll("a.nav-link");
        Assert.True(navLinks.Count >= 7); // At least 7 main navigation links
        
        // Verify specific navigation links
        Assert.Contains(navLinks, link => link.GetAttribute("href") == "/" && link.TextContent.Contains("Home"));
        Assert.Contains(navLinks, link => link.GetAttribute("href") == "requirements" && link.TextContent.Contains("Requirements"));
        Assert.Contains(navLinks, link => link.GetAttribute("href") == "testsuites" && link.TextContent.Contains("Test Suites"));
        Assert.Contains(navLinks, link => link.GetAttribute("href") == "testcases" && link.TextContent.Contains("Test Cases"));
        Assert.Contains(navLinks, link => link.GetAttribute("href") == "testplans" && link.TextContent.Contains("Test Plans"));
        Assert.Contains(navLinks, link => link.GetAttribute("href") == "test-run-sessions" && link.TextContent.Contains("Test Execution"));
        Assert.Contains(navLinks, link => link.GetAttribute("href") == "users" && link.TextContent.Contains("Users"));
    }

    [Fact]
    public void NavMenu_RendersCorrectly_WithProjectContext()
    {
        // Arrange
        var testProject = new ProjectDto { Id = 123, Name = "Test Project" };
        var mockProjectService = GetMockService<IProjectContextService>();
        mockProjectService.Setup(s => s.IsInProjectContext).Returns(true);
        mockProjectService.Setup(s => s.CurrentProject).Returns(testProject);

        // Act
        var component = RenderComponent<NavMenu>();
        
        // Assert
        Assert.Contains("TestFlow Pro", component.Markup); // Brand name
        
        // Check that Requirements link is project-aware
        var navLinks = component.FindAll("a.nav-link");
        Assert.Contains(navLinks, link => link.GetAttribute("href") == "projects/123/requirements" && link.TextContent.Contains("Requirements"));
        
        // Verify other navigation links are still present
        Assert.Contains(navLinks, link => link.GetAttribute("href") == "/" && link.TextContent.Contains("Home"));
        Assert.Contains(navLinks, link => link.GetAttribute("href") == "testsuites" && link.TextContent.Contains("Test Suites"));
        Assert.Contains(navLinks, link => link.GetAttribute("href") == "testcases" && link.TextContent.Contains("Test Cases"));
        Assert.Contains(navLinks, link => link.GetAttribute("href") == "testplans" && link.TextContent.Contains("Test Plans"));
        Assert.Contains(navLinks, link => link.GetAttribute("href") == "test-run-sessions" && link.TextContent.Contains("Test Execution"));
        Assert.Contains(navLinks, link => link.GetAttribute("href") == "users" && link.TextContent.Contains("Users"));
    }
    
    [Fact]
    public void NavMenu_StartsWithCollapsedState()
    {
        // Arrange
        var mockProjectService = GetMockService<IProjectContextService>();
        mockProjectService.Setup(s => s.IsInProjectContext).Returns(false);
        mockProjectService.Setup(s => s.CurrentProject).Returns((ProjectDto?)null);

        // Act
        var component = RenderComponent<NavMenu>();
        
        // Assert
        var navMenu = component.Find(".nav-scrollable");
        Assert.Contains("collapse", navMenu.GetAttribute("class") ?? "");
    }
    
    [Fact]
    public void NavMenu_TogglesVisibility_WhenToggleButtonClicked()
    {
        // Arrange
        var mockProjectService = GetMockService<IProjectContextService>();
        mockProjectService.Setup(s => s.IsInProjectContext).Returns(false);
        mockProjectService.Setup(s => s.CurrentProject).Returns((ProjectDto?)null);

        var component = RenderComponent<NavMenu>();
        var toggleButton = component.Find(".navbar-toggler");
        
        // Act
        toggleButton.Click();
        
        // Assert
        var navMenu = component.Find(".nav-scrollable");
        Assert.DoesNotContain("collapse", navMenu.GetAttribute("class") ?? "");
        
        // Act again
        toggleButton.Click();
        
        // Assert
        var navMenuAfter = component.Find(".nav-scrollable");
        Assert.Contains("collapse", navMenuAfter.GetAttribute("class") ?? "");
    }
    
    [Fact]
    public void NavMenu_CollapsesMenu_WhenNavMenuAreaClicked()
    {
        // Arrange
        var mockProjectService = GetMockService<IProjectContextService>();
        mockProjectService.Setup(s => s.IsInProjectContext).Returns(false);
        mockProjectService.Setup(s => s.CurrentProject).Returns((ProjectDto?)null);

        var component = RenderComponent<NavMenu>();
        var toggleButton = component.Find(".navbar-toggler");
        var navMenu = component.Find(".nav-scrollable");
        
        // First expand the menu
        toggleButton.Click();
        Assert.DoesNotContain("collapse", navMenu.GetAttribute("class") ?? "");
        
        // Act - Click on the nav menu area
        navMenu.Click();
        
        // Assert - Menu should be collapsed
        Assert.Contains("collapse", navMenu.GetAttribute("class") ?? "");
    }
    
    [Fact]
    public void NavMenu_HasCorrectBootstrapIcons()
    {
        // Arrange
        var mockProjectService = GetMockService<IProjectContextService>();
        mockProjectService.Setup(s => s.IsInProjectContext).Returns(false);
        mockProjectService.Setup(s => s.CurrentProject).Returns((ProjectDto?)null);

        // Act
        var component = RenderComponent<NavMenu>();
        
        // Assert - Check that each nav link has the correct icon class
        var homeIcon = component.Find("span.bi-house-door-fill-nav-menu");
        var requirementsIcon = component.Find("span.bi-file-earmark-text-nav-menu");
        var testSuitesIcon = component.Find("span.bi-collection-nav-menu");
        var testCasesIcon = component.Find("span.bi-file-earmark-check-nav-menu");
        var testPlansIcon = component.Find("span.bi-journal-code-nav-menu");
        var testExecutionIcon = component.Find("span.bi-play-circle-nav-menu");
        var usersIcon = component.Find("span.bi-person-lines-fill-nav-menu");
        
        Assert.NotNull(homeIcon);
        Assert.NotNull(requirementsIcon);
        Assert.NotNull(testSuitesIcon);
        Assert.NotNull(testCasesIcon);
        Assert.NotNull(testPlansIcon);
        Assert.NotNull(testExecutionIcon);
        Assert.NotNull(usersIcon);
    }
    
    [Fact]
    public void NavMenu_HomeLink_HasCorrectMatchAttribute()
    {
        // Arrange
        var mockProjectService = GetMockService<IProjectContextService>();
        mockProjectService.Setup(s => s.IsInProjectContext).Returns(false);
        mockProjectService.Setup(s => s.CurrentProject).Returns((ProjectDto?)null);

        // Act
        var component = RenderComponent<NavMenu>();
        
        // Assert
        var homeLink = component.Find("a.nav-link[href='/']");
        Assert.Contains("active", homeLink.GetAttribute("class") ?? "");

        var requirementsLink = component.Find("a.nav-link[href='requirements']");
        Assert.DoesNotContain("active", requirementsLink.GetAttribute("class") ?? "");
    }

    [Fact]
    public void NavMenu_RequirementsLink_UpdatesWhenProjectContextChanges()
    {
        // Arrange
        var testProject = new ProjectDto { Id = 456, Name = "Another Test Project" };
        var mockProjectService = GetMockService<IProjectContextService>();
        
        // Start without project context
        mockProjectService.Setup(s => s.IsInProjectContext).Returns(false);
        mockProjectService.Setup(s => s.CurrentProject).Returns((ProjectDto?)null);

        var component = RenderComponent<NavMenu>();
        
        // Assert initial state (no project context)
        var navLinks = component.FindAll("a.nav-link");
        Assert.Contains(navLinks, link => link.GetAttribute("href") == "requirements" && link.TextContent.Contains("Requirements"));
        
        // Act - Simulate project context change
        mockProjectService.Setup(s => s.IsInProjectContext).Returns(true);
        mockProjectService.Setup(s => s.CurrentProject).Returns(testProject);
        
        // Trigger the ProjectChanged event
        mockProjectService.Raise(s => s.ProjectChanged += null, testProject);
        
        // Re-render to get updated links
        component.Render();
        
        // Assert - Requirements link should now be project-aware
        var updatedNavLinks = component.FindAll("a.nav-link");
        Assert.Contains(updatedNavLinks, link => link.GetAttribute("href") == "projects/456/requirements" && link.TextContent.Contains("Requirements"));
    }
}