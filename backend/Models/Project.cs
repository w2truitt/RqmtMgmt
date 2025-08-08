using System.Collections.Generic;
using RqmtMgmtShared;

namespace backend.Models
{
    /// <summary>
    /// Represents a project that contains requirements, test suites, and test plans.
    /// Provides organizational structure and team-based access control.
    /// </summary>
    public class Project
    {
        /// <summary>
        /// Gets or sets the unique identifier for the project.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the project.
        /// </summary>
        public required string Name { get; set; }

        /// <summary>
        /// Gets or sets the project code used for generating requirement IDs.
        /// </summary>
        public required string Code { get; set; }

        /// <summary>
        /// Gets or sets the description of the project.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the status of the project.
        /// </summary>
        public ProjectStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the ID of the project owner.
        /// </summary>
        public int OwnerId { get; set; }

        /// <summary>
        /// Gets or sets the creation timestamp (UTC).
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the last updated timestamp (UTC), if applicable.
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Gets or sets the project owner user object.
        /// </summary>
        public User? Owner { get; set; }

        /// <summary>
        /// Gets or sets the collection of team members for this project.
        /// </summary>
        public ICollection<ProjectTeamMember> TeamMembers { get; set; } = new List<ProjectTeamMember>();

        /// <summary>
        /// Gets or sets the collection of requirements in this project.
        /// </summary>
        public ICollection<Requirement> Requirements { get; set; } = new List<Requirement>();

        /// <summary>
        /// Gets or sets the collection of test suites in this project.
        /// </summary>
        public ICollection<TestSuite> TestSuites { get; set; } = new List<TestSuite>();

        /// <summary>
        /// Gets or sets the collection of test plans in this project.
        /// </summary>
        public ICollection<TestPlan> TestPlans { get; set; } = new List<TestPlan>();
    }
}