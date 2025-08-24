using Microsoft.EntityFrameworkCore;
using backend.Models;

namespace backend.Data
{
    /// <summary>
    /// Database context for the Requirements and Test Management system.
    /// Provides DbSets for all core entities and configures relationships.
    /// </summary>
    public class RqmtMgmtDbContext : DbContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RqmtMgmtDbContext"/> class.
        /// </summary>
        public RqmtMgmtDbContext(DbContextOptions<RqmtMgmtDbContext> options) : base(options) { }

        /// <summary>Gets or sets the users table.</summary>
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        /// <summary>Gets or sets the projects table.</summary>
        public DbSet<Project> Projects { get; set; }
        /// <summary>Gets or sets the project team members table.</summary>
        public DbSet<ProjectTeamMember> ProjectTeamMembers { get; set; }
        /// <summary>Gets or sets the requirements table.</summary>
        public DbSet<Requirement> Requirements { get; set; }
        /// <summary>Gets or sets the requirement links table.</summary>
        public DbSet<RequirementLink> RequirementLinks { get; set; }
        /// <summary>Gets or sets the requirement version history table.</summary>
        public DbSet<RequirementVersion> RequirementVersions { get; set; }
        /// <summary>Gets or sets the test suites table.</summary>
        public DbSet<TestSuite> TestSuites { get; set; }
        /// <summary>Gets or sets the test plans table.</summary>
        public DbSet<TestPlan> TestPlans { get; set; }
        /// <summary>Gets or sets the test cases table.</summary>
        public DbSet<TestCase> TestCases { get; set; }
        /// <summary>Gets or sets the test case version history table.</summary>
        public DbSet<TestCaseVersion> TestCaseVersions { get; set; }
        /// <summary>Gets or sets the test plan/test case links table.</summary>
        public DbSet<TestPlanTestCase> TestPlanTestCases { get; set; }
        /// <summary>Gets or sets the requirement/test case links table.</summary>
        public DbSet<RequirementTestCaseLink> RequirementTestCaseLinks { get; set; }
        /// <summary>Gets or sets the test runs table.</summary>
        public DbSet<TestRun> TestRuns { get; set; }
        /// <summary>Gets or sets the test run sessions table.</summary>
        public DbSet<TestRunSession> TestRunSessions { get; set; }
        /// <summary>Gets or sets the test case executions table.</summary>
        public DbSet<TestCaseExecution> TestCaseExecutions { get; set; }
        /// <summary>Gets or sets the test step executions table.</summary>
        public DbSet<TestStepExecution> TestStepExecutions { get; set; }
        /// <summary>Gets or sets the audit logs table.</summary>
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<TestStep> TestSteps { get; set; }

        /// <summary>
        /// Configures the entity relationships and model conversions.
        /// </summary>
        /// <param name="modelBuilder">The model builder.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Composite keys
            modelBuilder.Entity<TestPlanTestCase>().HasKey(x => new { x.TestPlanId, x.TestCaseId });
            modelBuilder.Entity<RequirementTestCaseLink>().HasKey(x => new { x.RequirementId, x.TestCaseId });

            // Requirement self-referencing
            modelBuilder.Entity<Requirement>()
                .HasOne(r => r.Parent)
                .WithMany(r => r.Children)
                .HasForeignKey(r => r.ParentId)
                .OnDelete(DeleteBehavior.Restrict);

            // Requirement <-> User
            modelBuilder.Entity<Requirement>()
                .HasOne(r => r.Creator)
                .WithMany(u => u.RequirementsCreated)
                .HasForeignKey(r => r.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            // TestCase <-> TestSuite
            modelBuilder.Entity<TestCase>()
                .HasOne(tc => tc.Suite)
                .WithMany(ts => ts.TestCases)
                .HasForeignKey(tc => tc.SuiteId)
                .OnDelete(DeleteBehavior.SetNull);

            // TestCase <-> User
            modelBuilder.Entity<TestCase>()
                .HasOne(tc => tc.Creator)
                .WithMany(u => u.TestCasesCreated)
                .HasForeignKey(tc => tc.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            // TestSuite <-> User
            modelBuilder.Entity<TestSuite>()
                .HasOne(ts => ts.Creator)
                .WithMany(u => u.TestSuitesCreated)
                .HasForeignKey(ts => ts.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            // TestPlan <-> User
            modelBuilder.Entity<TestPlan>()
                .HasOne(tp => tp.Creator)
                .WithMany(u => u.TestPlansCreated)
                .HasForeignKey(tp => tp.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            // TestRun <-> TestCase
            modelBuilder.Entity<TestRun>()
                .HasOne(tr => tr.TestCase)
                .WithMany(tc => tc.TestRuns)
                .HasForeignKey(tr => tr.TestCaseId)
                .OnDelete(DeleteBehavior.Cascade);

            // TestRun <-> TestPlan
            modelBuilder.Entity<TestRun>()
                .HasOne(tr => tr.TestPlan)
                .WithMany()
                .HasForeignKey(tr => tr.TestPlanId)
                .OnDelete(DeleteBehavior.SetNull);

            // TestRun <-> User
            modelBuilder.Entity<TestRun>()
                .HasOne(tr => tr.Runner)
                .WithMany(u => u.TestRuns)
                .HasForeignKey(tr => tr.RunBy)
                .OnDelete(DeleteBehavior.Restrict);

            // TestRunSession relationships
            modelBuilder.Entity<TestRunSession>()
                .HasOne(trs => trs.TestPlan)
                .WithMany()
                .HasForeignKey(trs => trs.TestPlanId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TestRunSession>()
                .HasOne(trs => trs.Executor)
                .WithMany()
                .HasForeignKey(trs => trs.ExecutedBy)
                .OnDelete(DeleteBehavior.Restrict);

            // TestCaseExecution relationships
            modelBuilder.Entity<TestCaseExecution>()
                .HasOne(tce => tce.TestRunSession)
                .WithMany(trs => trs.TestCaseExecutions)
                .HasForeignKey(tce => tce.TestRunSessionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TestCaseExecution>()
                .HasOne(tce => tce.TestCase)
                .WithMany()
                .HasForeignKey(tce => tce.TestCaseId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TestCaseExecution>()
                .HasOne(tce => tce.Executor)
                .WithMany()
                .HasForeignKey(tce => tce.ExecutedBy)
                .OnDelete(DeleteBehavior.SetNull);

            // TestStepExecution relationships
            modelBuilder.Entity<TestStepExecution>()
                .HasOne(tse => tse.TestCaseExecution)
                .WithMany(tce => tce.TestStepExecutions)
                .HasForeignKey(tse => tse.TestCaseExecutionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TestStepExecution>()
                .HasOne(tse => tse.TestStep)
                .WithMany()
                .HasForeignKey(tse => tse.TestStepId)
                .OnDelete(DeleteBehavior.Restrict);

            // AuditLog <-> User
            modelBuilder.Entity<AuditLog>()
                .HasOne(al => al.User)
                .WithMany(u => u.AuditLogs)
                .HasForeignKey(al => al.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // RequirementLink navigation
            modelBuilder.Entity<RequirementLink>()
                .HasOne(rl => rl.FromRequirement)
                .WithMany(r => r.OutgoingLinks)
                .HasForeignKey(rl => rl.FromRequirementId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<RequirementLink>()
                .HasOne(rl => rl.ToRequirement)
                .WithMany(r => r.IncomingLinks)
                .HasForeignKey(rl => rl.ToRequirementId)
                .OnDelete(DeleteBehavior.Restrict);

            // Enum conversions
            modelBuilder.Entity<Requirement>()
                .Property(r => r.Type)
                .HasConversion<string>();
            modelBuilder.Entity<Requirement>()
                .Property(r => r.Status)
                .HasConversion<string>();
            modelBuilder.Entity<TestPlan>()
                .Property(tp => tp.Type)
                .HasConversion<string>();
            modelBuilder.Entity<TestRun>()
                .Property(tr => tr.Result)
                .HasConversion<string>();

            // New enum conversions for test execution tracking
            modelBuilder.Entity<TestRunSession>()
                .Property(trs => trs.Status)
                .HasConversion<string>();
            modelBuilder.Entity<TestCaseExecution>()
                .Property(tce => tce.OverallResult)
                .HasConversion<string>();
            modelBuilder.Entity<TestStepExecution>()
                .Property(tse => tse.Result)
                .HasConversion<string>();

            // Version tables
            modelBuilder.Entity<RequirementVersion>()
                .Property(v => v.Type)
                .HasConversion<string>();
            modelBuilder.Entity<RequirementVersion>()
                .Property(v => v.Status)
                .HasConversion<string>();

            // User <-> Role many-to-many
            modelBuilder.Entity<UserRole>().HasKey(ur => new { ur.UserId, ur.RoleId });
            modelBuilder.Entity<ProjectTeamMember>().HasKey(ptm => new { ptm.ProjectId, ptm.UserId });
            modelBuilder.Entity<ProjectTeamMember>()
                .HasOne(ptm => ptm.Project)
                .WithMany(p => p.TeamMembers)
                .HasForeignKey(ptm => ptm.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<ProjectTeamMember>()
                .HasOne(ptm => ptm.User)
                .WithMany()
                .HasForeignKey(ptm => ptm.UserId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId);
            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleId);

            // TestCase <-> TestStep (one-to-many, cascade delete)
            modelBuilder.Entity<TestCase>()
                .HasMany(tc => tc.Steps)
                .WithOne(ts => ts.TestCase)
                .HasForeignKey(ts => ts.TestCaseId)
                .OnDelete(DeleteBehavior.Cascade);

            // Performance Indexes
            modelBuilder.Entity<Requirement>()
                .HasIndex(r => new { r.Type, r.Status })
                .HasDatabaseName("IX_Requirements_Type_Status");

            modelBuilder.Entity<Requirement>()
                .HasIndex(r => r.CreatedAt)
                .HasDatabaseName("IX_Requirements_CreatedAt");

            modelBuilder.Entity<TestCase>()
                .HasIndex(tc => tc.CreatedAt)
                .HasDatabaseName("IX_TestCases_CreatedAt");

            modelBuilder.Entity<TestRun>()
                .HasIndex(tr => tr.RunAt)
                .HasDatabaseName("IX_TestRuns_RunAt");

            modelBuilder.Entity<TestRun>()
                .HasIndex(tr => tr.Result)
                .HasDatabaseName("IX_TestRuns_Result");

            // New performance indexes for test execution tracking
            modelBuilder.Entity<TestRunSession>()
                .HasIndex(trs => trs.Status)
                .HasDatabaseName("IX_TestRunSessions_Status");

            modelBuilder.Entity<TestRunSession>()
                .HasIndex(trs => trs.StartedAt)
                .HasDatabaseName("IX_TestRunSessions_StartedAt");

            modelBuilder.Entity<TestCaseExecution>()
                .HasIndex(tce => tce.OverallResult)
                .HasDatabaseName("IX_TestCaseExecutions_OverallResult");

            modelBuilder.Entity<TestCaseExecution>()
                .HasIndex(tce => tce.ExecutedAt)
                .HasDatabaseName("IX_TestCaseExecutions_ExecutedAt");

            modelBuilder.Entity<AuditLog>()
                .HasIndex(al => al.Timestamp)
                .HasDatabaseName("IX_AuditLogs_Timestamp");

            modelBuilder.Entity<AuditLog>()
                .HasIndex(al => new { al.Entity, al.EntityId })
                .HasDatabaseName("IX_AuditLogs_Entity_EntityId");

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique()
                .HasDatabaseName("IX_Users_Email");
        }
    }
}
