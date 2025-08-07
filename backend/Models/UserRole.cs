namespace backend.Models
{
    /// <summary>
    /// Represents the many-to-many relationship between users and roles.
    /// Used for managing user access control and authorization assignments.
    /// </summary>
    public class UserRole
    {
        /// <summary>
        /// Gets or sets the unique identifier of the user in this user-role relationship.
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Gets or sets the navigation property to the user entity.
        /// </summary>
        public User User { get; set; } = null!;

        /// <summary>
        /// Gets or sets the unique identifier of the role in this user-role relationship.
        /// </summary>
        public int RoleId { get; set; }

        /// <summary>
        /// Gets or sets the navigation property to the role entity.
        /// </summary>
        public Role Role { get; set; } = null!;
    }
}