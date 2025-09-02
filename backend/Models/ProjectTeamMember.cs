using RqmtMgmtShared;

namespace backend.Models
{
    /// <summary>
    /// Represents a team member's association with a project and their role within it.
    /// </summary>
    public class ProjectTeamMember
    {
        /// <summary>
        /// Gets or sets the project ID.
        /// </summary>
        public int ProjectId { get; set; }

        /// <summary>
        /// Gets or sets the user ID.
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Gets or sets the role of the user within the project.
        /// </summary>
        public ProjectRole Role { get; set; }

        /// <summary>
        /// Gets or sets when the user joined the project team.
        /// </summary>
        public DateTime JoinedAt { get; set; }

        /// <summary>
        /// Gets or sets whether the team member is currently active on the project.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets the navigation property to the project.
        /// </summary>
        public Project? Project { get; set; }

        /// <summary>
        /// Gets or sets the navigation property to the user.
        /// </summary>
        public User? User { get; set; }
    }
}