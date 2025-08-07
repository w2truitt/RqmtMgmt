namespace RqmtMgmtShared
{
    /// <summary>
    /// Data transfer object for user roles containing role information and permissions.
    /// Used for managing user access control and authorization within the system.
    /// </summary>
    public class RoleDto
    {
        /// <summary>
        /// Gets or sets the unique identifier for the role.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the role. This field is required and cannot be null.
        /// Examples include "Admin", "TestManager", "Developer", "Viewer".
        /// </summary>
        public required string Name { get; set; }
    }
}