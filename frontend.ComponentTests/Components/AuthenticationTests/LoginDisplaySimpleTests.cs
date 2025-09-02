using Bunit;
using frontend.Components.Authentication;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using MudBlazor.Services;
using Xunit;
using System.Security.Claims;
using Bunit.TestDoubles;

namespace frontend.ComponentTests.Components.AuthenticationTests;

/// <summary>
/// Tests for the simplified LoginDisplay component
/// </summary>
public class LoginDisplaySimpleTests : ComponentTestBase
{
    [Fact]
    public void LoginDisplay_Should_ShowAuthenticatedContent_WhenUserIsAuthenticated()
    {
        // Arrange - Use bUnit's test authorization context
        var authContext = this.AddTestAuthorization();
        authContext.SetAuthorized("test@example.com", AuthorizationState.Authorized);

        // Act - Wrap component in CascadingAuthenticationState
        var component = RenderComponent<CascadingAuthenticationState>(parameters => parameters
            .AddChildContent<LoginDisplay>()
        );

        // Assert - Check for user name and logout button
        Assert.Contains("Hello, test@example.com!", component.Markup);
        Assert.Contains("Log out", component.Markup);
    }

    [Fact]
    public void LoginDisplay_Should_ShowAnonymousContent_WhenUserIsNotAuthenticated()
    {
        // Arrange - Use bUnit's test authorization context
        var authContext = this.AddTestAuthorization();
        authContext.SetNotAuthorized();

        // Act - Wrap component in CascadingAuthenticationState
        var component = RenderComponent<CascadingAuthenticationState>(parameters => parameters
            .AddChildContent<LoginDisplay>()
        );

        // Assert - Check for login button, no user-specific information
        Assert.Contains("Log in", component.Markup);
        Assert.DoesNotContain("Hello,", component.Markup);
        Assert.DoesNotContain("Log out", component.Markup);
    }

    [Fact]
    public void LoginDisplay_Should_ShowUsername_WhenUserIsAuthenticated()
    {
        // Arrange - Use bUnit's test authorization context
        var authContext = this.AddTestAuthorization();
        authContext.SetAuthorized("test@example.com", AuthorizationState.Authorized);

        // Act
        var component = RenderComponent<LoginDisplay>();

        // Debug - output the actual markup to understand what's rendered
        System.Console.WriteLine($"Actual markup: {component.Markup}");

        // Assert - component should contain the user's name
        Assert.Contains("test@example.com", component.Markup);
    }

    // Note: bUnit doesn't support testing authentication state changes within a single test
    // because components don't automatically re-render when auth state changes after initial render.
    // Each authentication state is tested separately in other tests.
}
