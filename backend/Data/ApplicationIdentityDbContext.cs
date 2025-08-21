using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using backend.Models;

namespace backend.Data
{
    /// <summary>
    /// Identity database context for ASP.NET Core Identity and Duende IdentityServer.
    /// Handles user authentication, authorization, and identity management.
    /// </summary>
    public class ApplicationIdentityDbContext : IdentityDbContext<ApplicationUser>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationIdentityDbContext"/> class.
        /// </summary>
        /// <param name="options">The database context options.</param>
        public ApplicationIdentityDbContext(DbContextOptions<ApplicationIdentityDbContext> options) : base(options)
        {
        }

        /// <summary>
        /// Configures the model that was discovered by convention from the entity types.
        /// </summary>
        /// <param name="builder">The model builder used to construct the model for this context.</param>
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure ApplicationUser entity
            builder.Entity<ApplicationUser>(entity =>
            {
                entity.Property(e => e.FirstName).HasMaxLength(100);
                entity.Property(e => e.LastName).HasMaxLength(100);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");
            });

            // Rename Identity tables to avoid conflicts with existing User/Role tables
            builder.Entity<ApplicationUser>().ToTable("AspNetUsers");
            builder.Entity<IdentityRole>().ToTable("AspNetRoles");
            builder.Entity<IdentityUserRole<string>>().ToTable("AspNetUserRoles");
            builder.Entity<IdentityUserClaim<string>>().ToTable("AspNetUserClaims");
            builder.Entity<IdentityUserLogin<string>>().ToTable("AspNetUserLogins");
            builder.Entity<IdentityUserToken<string>>().ToTable("AspNetUserTokens");
            builder.Entity<IdentityRoleClaim<string>>().ToTable("AspNetRoleClaims");
        }
    }
}
