using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace frontend.ComponentTests.TestHelpers;

/// <summary>
/// Test implementation of AuthenticationStateProvider for component testing
/// </summary>
public class TestAuthStateProvider : AuthenticationStateProvider
{
    private AuthenticationState _currentState = new(new ClaimsPrincipal(new ClaimsIdentity()));

    public TestAuthStateProvider()
    {
        // Default to anonymous user
        SetUser(new ClaimsPrincipal(new ClaimsIdentity()));
    }

    /// <summary>
    /// Sets the current user for testing
    /// </summary>
    /// <param name="user">The user to set as authenticated</param>
    public void SetUser(ClaimsPrincipal user)
    {
        _currentState = new AuthenticationState(user);
        NotifyAuthenticationStateChanged(Task.FromResult(_currentState));
    }

    /// <summary>
    /// Sets an authenticated user with the specified email and roles
    /// </summary>
    /// <param name="email">User email</param>
    /// <param name="roles">User roles</param>
    public void SetAuthenticatedUser(string email, params string[] roles)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, email),
            new Claim(ClaimTypes.Email, email)
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var identity = new ClaimsIdentity(claims, "Test");
        var user = new ClaimsPrincipal(identity);
        SetUser(user);
    }

    /// <summary>
    /// Sets the user as anonymous (not authenticated)
    /// </summary>
    public void SetAnonymousUser()
    {
        SetUser(new ClaimsPrincipal(new ClaimsIdentity()));
    }

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        return Task.FromResult(_currentState);
    }
}
