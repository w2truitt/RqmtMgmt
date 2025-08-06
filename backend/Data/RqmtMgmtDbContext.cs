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

            // Version tables
            modelBuilder.Entity<RequirementVersion>()
                .Property(v => v.Type)
                .HasConversion<string>();
            modelBuilder.Entity<RequirementVersion>()
                .Property(v => v.Status)
                .HasConversion<string>();

            // (removed misplaced declarations)
            // User <-> Role many-to-many
            modelBuilder.Entity<UserRole>().HasKey(ur => new { ur.UserId, ur.RoleId });
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