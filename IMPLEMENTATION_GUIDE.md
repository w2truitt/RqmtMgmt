# User ID Mapping Fix - Implementation Guide

## Issue Summary
The "No Projects Found" issue occurs because the JWT token's `sub` claim contains a string identifier (email/username) but the backend expects an integer user ID. This causes the membership filter to fail.

## Implementation Steps

### 1. Enhanced GetCurrentUserId Method

**File**: `backend/Controllers/ProjectsController.cs`

**Action**: Replace the existing `GetCurrentUserId()` method (around line 283) with this enhanced version:

```csharp
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
        
        // First try: Parse as integer (for tokens that already include integer user IDs)
        if (int.TryParse(userIdClaim.Value, out var userId))
        {
            Console.WriteLine($"[DEBUG] GetCurrentUserId - Successfully parsed user ID: {userId}");
            return userId;
        }
        else
        {
            Console.WriteLine($"[DEBUG] GetCurrentUserId - Failed to parse user ID claim value: {userIdClaim.Value}");
            
            // Second try: If sub claim is not an integer, try to look up user by email
            // This handles cases where the sub claim contains the user's email
            try
            {
                using var scope = HttpContext.RequestServices.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<backend.Data.RqmtMgmtDbContext>();
                
                var user = context.Users
                    .Where(u => u.UserName == userIdClaim.Value || u.Email == userIdClaim.Value)
                    .Select(u => new { u.Id })
                    .FirstOrDefault();
                    
                if (user != null)
                {
                    Console.WriteLine($"[DEBUG] GetCurrentUserId - Found user by sub claim lookup: {user.Id}");
                    return user.Id;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DEBUG] GetCurrentUserId - Database lookup error: {ex.Message}");
            }
        }
    }
    else
    {
        Console.WriteLine("[DEBUG] GetCurrentUserId - No user ID claim found in standard locations");
    }
    
    // Third try: Get user ID by looking up the user by email from the token
    var emailClaim = User.FindFirst("email") ?? User.FindFirst(System.Security.Claims.ClaimTypes.Email);
    if (emailClaim != null)
    {
        Console.WriteLine($"[DEBUG] GetCurrentUserId - Found email claim: {emailClaim.Value}, attempting user lookup");
        try
        {
            using var scope = HttpContext.RequestServices.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<backend.Data.RqmtMgmtDbContext>();
            
            var user = context.Users
                .Where(u => u.Email == emailClaim.Value)
                .Select(u => new { u.Id })
                .FirstOrDefault();
                
            if (user != null)
            {
                Console.WriteLine($"[DEBUG] GetCurrentUserId - Found user by email lookup: {user.Id}");
                return user.Id;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DEBUG] GetCurrentUserId - Email lookup error: {ex.Message}");
        }
    }
    
    // Fourth try: Check for X-User-Id header for development/testing scenarios
    if (HttpContext.Items.TryGetValue("UserId", out var impersonatedUserId) && 
        int.TryParse(impersonatedUserId?.ToString(), out var impersonatedId))
    {
        Console.WriteLine($"[DEBUG] GetCurrentUserId - Using impersonated user ID: {impersonatedId}");
        return impersonatedId;
    }
    
    Console.WriteLine("[DEBUG] GetCurrentUserId - No valid user ID found, returning null");
    return null;
}
```

**Also add** the following using statement at the top of the file:
```csharp
using backend.Data;
```

### 2. Enhanced GetProjects Method Debugging

**File**: `backend/Controllers/ProjectsController.cs`

**Action**: Add debugging statements to the GetProjects method around line 40:

```csharp
// Extract current user ID from JWT token for UserIsMember filter
if (filter.UserIsMember.HasValue && filter.UserIsMember.Value)
{
    Console.WriteLine("[DEBUG] GetProjects - UserIsMember filter requested");
    var currentUserId = GetCurrentUserId();
    if (currentUserId.HasValue)
    {
        Console.WriteLine($"[DEBUG] GetProjects - Setting CurrentUserId to: {currentUserId.Value}");
        filter.CurrentUserId = currentUserId.Value;
    }
    else
    {
        Console.WriteLine("[DEBUG] GetProjects - No valid user ID found, returning empty result");
        // If UserIsMember is requested but no valid user ID found, return empty result
        return Ok(new PagedResult<ProjectDto>
        {
            Items = new List<ProjectDto>(),
            TotalItems = 0,
            PageNumber = filter.Page,
            PageSize = filter.PageSize
        });
    }
}

var result = await _projectService.GetProjectsAsync(filter);
Console.WriteLine($"[DEBUG] GetProjects - Returning {result.Items.Count} projects (Total: {result.TotalItems})");
return Ok(result);
```

### 3. Database Seeder Enhancement

**File**: `backend/Data/DatabaseSeeder.cs`

**Action**: Add this code in the `SeedTestDataAsync` method before the "Sample Audit Log" section (around line 270):

```csharp
// Create project team members for testing the membership filter
var testerUser = await context.Users.FirstOrDefaultAsync(u => u.Email == "tester@rqmtmgmt.local");
var devUser = await context.Users.FirstOrDefaultAsync(u => u.Email == "dev@rqmtmgmt.local");
var pmUser = await context.Users.FirstOrDefaultAsync(u => u.Email == "pm@rqmtmgmt.local");

var projectTeamMembers = new List<ProjectTeamMember>();

// Add tester as QA Engineer
if (testerUser != null)
{
    projectTeamMembers.Add(new ProjectTeamMember
    {
        ProjectId = defaultProject.Id,
        UserId = testerUser.Id,
        Role = ProjectRole.QAEngineer,
        JoinedAt = DateTime.UtcNow,
        IsActive = true
    });
}

// Add developer as Engineer
if (devUser != null)
{
    projectTeamMembers.Add(new ProjectTeamMember
    {
        ProjectId = defaultProject.Id,
        UserId = devUser.Id,
        Role = ProjectRole.Engineer,
        JoinedAt = DateTime.UtcNow,
        IsActive = true
    });
}

// Add PM as Product Owner
if (pmUser != null)
{
    projectTeamMembers.Add(new ProjectTeamMember
    {
        ProjectId = defaultProject.Id,
        UserId = pmUser.Id,
        Role = ProjectRole.ProductOwner,
        JoinedAt = DateTime.UtcNow,
        IsActive = true
    });
}

if (projectTeamMembers.Any())
{
    await context.ProjectTeamMembers.AddRangeAsync(projectTeamMembers);
    await context.SaveChangesAsync();
}
```

## Testing Steps

1. **Apply the changes** above to the respective files
2. **Restart Docker containers** to rebuild with the changes:
   ```bash
   cd docker-compose
   docker-compose down
   docker-compose up -d
   ```
3. **Login** as `tester@rqmtmgmt.local` with password `Test123!`
4. **Check console logs** for debug output from the enhanced GetCurrentUserId method
5. **Test the membership filter**:
   - Go to Projects page
   - Select "My Projects Only" from the membership filter dropdown
   - Should now show the "Legacy Requirements" project where tester is a team member

## Expected Debug Output

When the fix is working correctly, you should see console output like:
```
[DEBUG] GetCurrentUserId - All available claims: sub=tester@rqmtmgmt.local, email=tester@rqmtmgmt.local, name=Quality Tester, role=Tester
[DEBUG] GetCurrentUserId - Found user ID claim: sub=tester@rqmtmgmt.local
[DEBUG] GetCurrentUserId - Failed to parse user ID claim value: tester@rqmtmgmt.local
[DEBUG] GetCurrentUserId - Found user by sub claim lookup: 4
[DEBUG] GetProjects - UserIsMember filter requested
[DEBUG] GetProjects - Setting CurrentUserId to: 4
[DEBUG] GetProjects - Returning 1 projects (Total: 1)
```

## Verification

After applying the fix:
- ✅ The membership filter should work correctly
- ✅ "My Projects Only" should show projects where the user is a team member
- ✅ All users (tester, dev, pm) should see the "Legacy Requirements" project when using "My Projects Only"
- ✅ The debug output should show successful user ID resolution

This fix is backward compatible and handles both integer and string-based JWT claims, making the system resilient to different authentication provider configurations.