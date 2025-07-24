using Microsoft.EntityFrameworkCore;
using backend.Models;

namespace backend.Data
{
    public class RqmtMgmtDbContext : DbContext
    {
        public RqmtMgmtDbContext(DbContextOptions<RqmtMgmtDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Requirement> Requirements { get; set; }
        public DbSet<RequirementLink> RequirementLinks { get; set; }
        public DbSet<TestSuite> TestSuites { get; set; }
        public DbSet<TestPlan> TestPlans { get; set; }
        public DbSet<TestCase> TestCases { get; set; }
        public DbSet<TestPlanTestCase> TestPlanTestCases { get; set; }
        public DbSet<RequirementTestCaseLink> RequirementTestCaseLinks { get; set; }
        public DbSet<TestRun> TestRuns { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TestPlanTestCase>().HasKey(x => new { x.TestPlanId, x.TestCaseId });
            modelBuilder.Entity<RequirementTestCaseLink>().HasKey(x => new { x.RequirementId, x.TestCaseId });
        }
    }
}
