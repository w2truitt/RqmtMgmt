using System.Collections.Generic;

namespace backend.Models
{
    /// <summary>
    /// Represents a user of the requirements and test management system.
    /// </summary>
    public class User
    {
        /// <summary>
        /// Gets or sets the unique identifier for the user.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the username of the user.
        /// </summary>
        public required string UserName { get; set; }

        /// <summary>
        /// Gets or sets the email address of the user.
        /// </summary>
        public required string Email { get; set; }


        /// <summary>
        /// Gets or sets the roles associated with this user.
        /// </summary>
        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

        /// <summary>
        /// Gets or sets the creation timestamp (UTC).
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the collection of requirements created by this user.
        /// </summary>
        public ICollection<Requirement> RequirementsCreated { get; set; } = new List<Requirement>();

        /// <summary>
        /// Gets or sets the collection of test cases created by this user.
        /// </summary>
        public ICollection<TestCase> TestCasesCreated { get; set; } = new List<TestCase>();

        /// <summary>
        /// Gets or sets the collection of test suites created by this user.
        /// </summary>
        public ICollection<TestSuite> TestSuitesCreated { get; set; } = new List<TestSuite>();

        /// <summary>
        /// Gets or sets the collection of test plans created by this user.
        /// </summary>
        public ICollection<TestPlan> TestPlansCreated { get; set; } = new List<TestPlan>();

        /// <summary>
        /// Gets or sets the collection of test runs executed by this user.
        /// </summary>
        public ICollection<TestRun> TestRuns { get; set; } = new List<TestRun>();

        /// <summary>
        /// Gets or sets the collection of audit logs for this user.
        /// </summary>
        public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
    }
}
