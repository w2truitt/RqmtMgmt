using Microsoft.AspNetCore.Identity;
using backend.Models;
using backend.Data;
using Microsoft.EntityFrameworkCore;

namespace backend.Data
{
    /// <summary>
    /// Provides seed data for ASP.NET Core Identity users and roles.
    /// Creates an initial admin user for development and testing purposes.
    /// </summary>
    public static class IdentitySeedData
    {
        /// <summary>
        /// Seeds the identity database with default admin user and roles.
        /// </summary>
        /// <param name="serviceProvider">The service provider to resolve dependencies.</param>
        /// <param name="context">The identity database context.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public static async Task SeedAsync(IServiceProvider serviceProvider, ApplicationIdentityDbContext context)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // Ensure database is created
            await context.Database.EnsureCreatedAsync();

            // Seed roles
            await SeedRolesAsync(roleManager);

            // Seed admin user
            await SeedAdminUserAsync(userManager);
        }

        /// <summary>
        /// Seeds the default roles for the application.
        /// </summary>
        /// <param name="roleManager">The role manager service.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            string[] roles = { "Admin", "Manager", "Developer", "Tester", "Viewer" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }

        /// <summary>
        /// Seeds the default admin user for the application.
        /// </summary>
        /// <param name="userManager">The user manager service.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private static async Task SeedAdminUserAsync(UserManager<ApplicationUser> userManager)
        {
            const string adminEmail = "admin@rqmtmgmt.local";
            const string adminPassword = "Admin123!"; // Change this in production!

            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    FirstName = "System",
                    LastName = "Administrator",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var result = await userManager.CreateAsync(adminUser, adminPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                    
                    // Add additional claims for the admin user
                    await userManager.AddClaimAsync(adminUser, new System.Security.Claims.Claim("name", adminUser.FullName));
                    await userManager.AddClaimAsync(adminUser, new System.Security.Claims.Claim("email", adminUser.Email));
                    await userManager.AddClaimAsync(adminUser, new System.Security.Claims.Claim("role", "Admin"));
                }
                else
                {
                    throw new Exception($"Failed to create admin user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
        }
    }
}
