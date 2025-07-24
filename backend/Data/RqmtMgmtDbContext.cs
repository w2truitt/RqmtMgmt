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
        }
    }
}
