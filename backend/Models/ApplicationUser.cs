using Microsoft.AspNetCore.Identity;

namespace backend.Models
{
    /// <summary>
    /// Application user model extending IdentityUser for additional properties.
    /// Integrates with ASP.NET Core Identity and Duende IdentityServer.
    /// </summary>
    public class ApplicationUser : IdentityUser
    {
        /// <summary>
        /// Gets or sets the user's first name.
        /// </summary>
        public string? FirstName { get; set; }

        /// <summary>
        /// Gets or sets the user's last name.
        /// </summary>
        public string? LastName { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the user was created.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the date and time when the user was last updated.
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets the user's full name by combining first and last name.
        /// </summary>
        public string FullName => $"{FirstName} {LastName}".Trim();
    }
}
