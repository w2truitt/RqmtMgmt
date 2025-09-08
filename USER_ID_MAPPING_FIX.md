# User ID Mapping Issue Fix

## Problem Analysis

The membership filter shows "No Projects Found" because there's a user ID mapping issue between:
- Frontend user: `tester@rqmtmgmt.local` (from JWT token)
- Backend team member: `tester.rqmtmgmt` (mentioned in the issue)

## Root Cause

The issue is in the JWT token claim mapping. The IdentityServer likely includes the user's email or username in the `sub` claim, but the backend expects an integer user ID to match against the `ProjectTeamMembers.UserId` field.

## Solution Steps

### 1. Enhanced GetCurrentUserId Method

Replace the `GetCurrentUserId()` method in `backend/Controllers/ProjectsController.cs` with this enhanced version:

```csharp
/// <summary>
/// Extracts the current user ID from the JWT token claims with enhanced debugging and fallback logic.
/// </summary>
/// <returns>The current user ID if found and valid; otherwise, null.</returns>
private int? GetCurrentUserId()
{
    // Enhanced debugging for user ID extraction from JWT token claims
    var allClaims = User.Claims.Select(c => $"{c.Type}={c.Value}").ToList();
    Console.WriteLine($"[DEBUG] GetCurrentUserId - All available claims: {string.Join(", ", allClaims)}");
    
    // Try to get user ID from 'sub' claim (standard JWT claim for subject)
    var userIdClaim = User.FindFirst("sub") ?? User.FindFirst("user_id") ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
    
    if (userIdClaim != null)
    {
        Console.WriteLine($"[DEBUG] GetCurrentUserId - Found user ID claim: {userIdClaim.Type}={userIdClaim.Value}");
        if (int.TryParse(userIdClaim.Value, out var userId))
        {
            Console.WriteLine($"[DEBUG] GetCurrentUserId - Successfully parsed user ID: {userId}");
            return userId;
        }
        else
        {
            Console.WriteLine($"[DEBUG] GetCurrentUserId - Failed to parse user ID claim value: {userIdClaim.Value}");
            
            // If sub claim is not an integer, it might be a username or email
            // Try to look up the user by the sub claim value
            var userBySubClaim = LookupUserByIdentifier(userIdClaim.Value);
            if (userBySubClaim.HasValue)
            {
                Console.WriteLine($"[DEBUG] GetCurrentUserId - Found user by sub claim lookup: {userBySubClaim.Value}");
                return userBySubClaim.Value;
            }
        }
    }
    else
    {
        Console.WriteLine("[DEBUG] GetCurrentUserId - No user ID claim found in standard locations");
    }
    
    // Try to get user ID by looking up the user by email from the token
    var emailClaim = User.FindFirst("email") ?? User.FindFirst(System.Security.Claims.ClaimTypes.Email);
    if (emailClaim != null)
    {
        Console.WriteLine($"[DEBUG] GetCurrentUserId - Found email claim: {emailClaim.Value}, attempting user lookup");
        var userByEmail = LookupUserByEmail(emailClaim.Value);
        if (userByEmail.HasValue)
        {
            Console.WriteLine($"[DEBUG] GetCurrentUserId - Found user by email lookup: {userByEmail.Value}");
            return userByEmail.Value;
        }
    }
    
    // Also check for X-User-Id header for development/testing scenarios
    if (HttpContext.Items.TryGetValue("UserId", out var impersonatedUserId) && 
        int.TryParse(impersonatedUserId?.ToString(), out var impersonatedId))
    {
        Console.WriteLine($"[DEBUG] GetCurrentUserId - Using impersonated user ID: {impersonatedId}");
        return impersonatedId;
    }
    
    Console.WriteLine("[DEBUG] GetCurrentUserId - No valid user ID found, returning null");
    return null;
}

/// <summary>
/// Looks up a user by identifier (username or email) synchronously.
/// </summary>
private int? LookupUserByIdentifier(string identifier)
{
    try
    {
        using var scope = HttpContext.RequestServices.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<RqmtMgmtDbContext>();
        
        var user = context.Users
            .Where(u => u.UserName == identifier || u.Email == identifier)
            .Select(u => new { u.Id })
            .FirstOrDefault();
            
        return user?.Id;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[DEBUG] LookupUserByIdentifier - Error: {ex.Message}");
        return null;
    }
}

/// <summary>
/// Looks up a user by email synchronously.
/// </summary>
private int? LookupUserByEmail(string email)
{
    try
    {
        using var scope = HttpContext.RequestServices.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<RqmtMgmtDbContext>();
        
        var user = context.Users
            .Where(u => u.Email == email)
            .Select(u => new { u.Id })
            .FirstOrDefault();
            
        return user?.Id;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[DEBUG] LookupUserByEmail - Error: {ex.Message}");
        return null;
    }
}
```

### 2. IdentityServer Configuration Fix

Update the IdentityServer configuration to include the user ID in the JWT token claims. In `identityserver/Config.cs`, ensure the API resource includes the user ID claim:

```csharp
new ApiResource("rqmtmgmt-api", "Requirements Management API")
{
    Scopes = { "rqmtmgmt.api" },
    UserClaims = new List<string> { "role", "email", "name", "sub", "user_id" },
    ShowInDiscoveryDocument = true
}
```

### 3. Custom Profile Service

Create a custom profile service to ensure the correct user ID is included in the token. Create `identityserver/Services/ProfileService.cs`:

```csharp
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using identityserver.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace identityserver.Services
{
    public class ProfileService : IProfileService
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public ProfileService(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            var user = await _userManager.GetUserAsync(context.Subject);
            if (user != null)
            {
                var claims = new List<Claim>
                {
                    new Claim("user_id", user.Id),
                    new Claim("email", user.Email ?? ""),
                    new Claim("name", user.Name ?? "")
                };

                var roles = await _userManager.GetRolesAsync(user);
                claims.AddRange(roles.Select(role => new Claim("role", role)));

                context.IssuedClaims.AddRange(claims);
            }
        }

        public async Task IsActiveAsync(IsActiveContext context)
        {
            var user = await _userManager.GetUserAsync(context.Subject);
            context.IsActive = user != null;
        }
    }
}
```

Register this service in `identityserver/HostingExtensions.cs`:

```csharp
builder.Services.AddTransient<IProfileService, ProfileService>();
```

### 4. Database Seeder Enhancement

Ensure the DatabaseSeeder creates proper project team members. Add this to `backend/Data/DatabaseSeeder.cs` in the `SeedTestDataAsync` method:

```csharp
// Create project team members for testing
var testerUser = await context.Users.FirstOrDefaultAsync(u => u.Email == "tester@rqmtmgmt.local");
if (testerUser != null)
{
    var projectTeamMember = new ProjectTeamMember
    {
        ProjectId = defaultProject.Id,
        UserId = testerUser.Id,
        Role = ProjectRole.QAEngineer,
        JoinedAt = DateTime.UtcNow,
        IsActive = true
    };

    await context.ProjectTeamMembers.AddAsync(projectTeamMember);
    await context.SaveChangesAsync();
}
```

## Testing Steps

1. Restart the Docker containers to rebuild with the changes
2. Log in as `tester@rqmtmgmt.local`
3. Check the console output for debug messages from `GetCurrentUserId`
4. Try the "My Projects Only" filter on the Projects page
5. Verify that projects with the tester as a team member are displayed

## Expected Outcome

After applying these fixes:
- The JWT token will include the correct user ID claim
- The GetCurrentUserId method will successfully extract the user ID
- The membership filter will work correctly
- "My Projects Only" will show projects where the user is a team member