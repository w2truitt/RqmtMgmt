using Bunit;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Microsoft.AspNetCore.Components;
using Bunit.TestDoubles;

namespace frontend.ComponentTests.Components.AuthenticationTests;

/// <summary>
/// Tests for the AuthorizeView component behavior
/// </summary>
public class AuthorizeViewTests : ComponentTestBase
{
    [Fact]
    public void AuthorizeView_Should_ShowAuthorizedContent_WhenUserIsAuthenticated()
    {
        // Arrange - Use bUnit's test authorization context
        var authContext = this.AddTestAuthorization();
        authContext.SetAuthorized("test@example.com", AuthorizationState.Authorized);

        // Act - Use the proper syntax for AuthorizeView with generic ChildContent
        var component = RenderComponent<AuthorizeView>(parameters => parameters
            .Add(p => p.ChildContent, context => builder => builder.AddContent(0, "Authorized Content"))
        );

        // Assert
        Assert.Contains("Authorized Content", component.Markup);
    }

    [Fact]
    public void AuthorizeView_Should_ShowNothing_WhenUserIsNotAuthenticated_AndNoNotAuthorizedContent()
    {
        // Arrange - Use bUnit's test authorization context
        var authContext = this.AddTestAuthorization();
        authContext.SetNotAuthorized();

        // Act - Use the proper syntax for AuthorizeView, only specify authorized content
        var component = RenderComponent<AuthorizeView>(parameters => parameters
            .Add(p => p.ChildContent, context => builder => builder.AddContent(0, "Authorized Content"))
        );

        // Assert - With anonymous user, should not show authorized content (empty markup)
        Assert.DoesNotContain("Authorized Content", component.Markup);
    }
}
