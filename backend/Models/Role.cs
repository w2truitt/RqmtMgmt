using System.Collections.Generic;

namespace backend.Models
{
    /// <summary>
    /// Represents a user role in the system for access control and authorization.
    /// Defines permissions and capabilities that can be assigned to users.
    /// </summary>
    public class Role
    {
        /// <summary>
        /// Gets or sets the unique identifier for the role.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the role.
        /// Examples include "Admin", "TestManager", "Developer", "Viewer".
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the collection of user-role associations for this role.
        /// </summary>
        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    }
}