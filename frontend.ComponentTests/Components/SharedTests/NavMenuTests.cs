using Bunit;
using frontend.Layout;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using AngleSharp.Dom;

namespace frontend.ComponentTests.Components.SharedTests;

/// <summary>
/// Tests for the NavMenu component
/// </summary>
public class NavMenuTests : ComponentTestBase
{
    [Fact]
    public void NavMenu_RendersCorrectly_WithAllNavigationLinks()
    {
        // Act
        var component = RenderComponent<NavMenu>();
        
        // Assert
        Assert.Contains("frontend", component.Markup); // Brand name
        
        // Check all navigation links are present
        var navLinks = component.FindAll("a.nav-link");
        Assert.Equal(6, navLinks.Count);
        
        // Verify specific navigation links
        Assert.Contains(navLinks, link => link.GetAttribute("href") == "/" && link.TextContent.Contains("Home"));
        Assert.Contains(navLinks, link => link.GetAttribute("href") == "requirements" && link.TextContent.Contains("Requirements"));
        Assert.Contains(navLinks, link => link.GetAttribute("href") == "testsuites" && link.TextContent.Contains("Test Suites"));
        Assert.Contains(navLinks, link => link.GetAttribute("href") == "testcases" && link.TextContent.Contains("Test Cases"));
        Assert.Contains(navLinks, link => link.GetAttribute("href") == "testplans" && link.TextContent.Contains("Test Plans"));
        Assert.Contains(navLinks, link => link.GetAttribute("href") == "users" && link.TextContent.Contains("Users"));
    }
    
    [Fact]
    public void NavMenu_StartsWithCollapsedState()
    {
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
        // Act
        var component = RenderComponent<NavMenu>();
        
        // Assert - Check that each nav link has the correct icon class
        var homeIcon = component.Find("span.bi-house-door-fill-nav-menu");
        var requirementsIcon = component.Find("span.bi-file-earmark-text-nav-menu");
        var testSuitesIcon = component.Find("span.bi-collection-nav-menu");
        var testCasesIcon = component.Find("span.bi-file-earmark-check-nav-menu");
        var testPlansIcon = component.Find("span.bi-journal-code-nav-menu");
        var usersIcon = component.Find("span.bi-person-lines-fill-nav-menu");
        
        Assert.NotNull(homeIcon);
        Assert.NotNull(requirementsIcon);
        Assert.NotNull(testSuitesIcon);
        Assert.NotNull(testCasesIcon);
        Assert.NotNull(testPlansIcon);
        Assert.NotNull(usersIcon);
    }
    
    [Fact]
    public void NavMenu_HomeLink_HasCorrectMatchAttribute()
    {
        // Act
        var component = RenderComponent<NavMenu>();
        
        // Assert
        var homeLink = component.Find("a.nav-link[href='/']");
        Assert.Contains("active", homeLink.GetAttribute("class") ?? "");

        var requirementsLink = component.Find("a.nav-link[href='requirements']");
        Assert.DoesNotContain("active", requirementsLink.GetAttribute("class") ?? "");
    }
}