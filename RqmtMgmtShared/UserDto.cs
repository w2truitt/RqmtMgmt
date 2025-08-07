using System.Collections.Generic;

namespace RqmtMgmtShared
{
    /// <summary>
    /// Data transfer object for user information including associated roles.
    /// </summary>
    public class UserDto
    {
        /// <summary>
        /// Gets or sets the unique identifier for the user.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the username for the user. This field is required and cannot be null.
        /// </summary>
        public required string UserName { get; set; }

        /// <summary>
        /// Gets or sets the email address for the user. This field is required and cannot be null.
        /// </summary>
        public required string Email { get; set; }

        /// <summary>
        /// Gets or sets the collection of role names assigned to this user.
        /// </summary>
        public List<string> Roles { get; set; } = new();
    }
}