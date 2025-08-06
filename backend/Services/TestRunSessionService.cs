using Microsoft.EntityFrameworkCore;
using RqmtMgmtShared;
using backend.Data;
using backend.Models;

namespace backend.Services
{
    /// <summary>
    /// Service for test run session management operations
    /// </summary>
    public class TestRunSessionService : ITestRunSessionService
    {
        private readonly RqmtMgmtDbContext _context;

        public TestRunSessionService(RqmtMgmtDbContext context)
        {
            _context = context;
        }

        public async Task<List<TestRunSessionDto>> GetAllAsync()
        {
            var sessions = await _context.TestRunSessions
                .Include(trs => trs.TestPlan)
                .Include(trs => trs.Executor)
                .Include(trs => trs.TestCaseExecutions)
                .OrderByDescending(trs => trs.StartedAt)
                .ToListAsync();

            return sessions.Select(MapToDto).ToList();
        }

        public async Task<TestRunSessionDto?> GetByIdAsync(int id)
        {
            var session = await _context.TestRunSessions
                .Include(trs => trs.TestPlan)
                .Include(trs => trs.Executor)
                .Include(trs => trs.TestCaseExecutions)
                    .ThenInclude(tce => tce.TestCase)
                .Include(trs => trs.TestCaseExecutions)
                    .ThenInclude(tce => tce.TestStepExecutions)
                        .ThenInclude(tse => tse.TestStep)
                .FirstOrDefaultAsync(trs => trs.Id == id);

            return session != null ? MapToDto(session) : null;
        }

        public async Task<TestRunSessionDto?> CreateAsync(TestRunSessionDto testRunSession)
        {
            var entity = new TestRunSession
            {
                Name = testRunSession.Name,
                Description = testRunSession.Description,
                TestPlanId = testRunSession.TestPlanId,
                ExecutedBy = testRunSession.ExecutedBy,
                StartedAt = testRunSession.StartedAt,
                CompletedAt = testRunSession.CompletedAt,
                Status = testRunSession.Status,
                Environment = testRunSession.Environment,
                BuildVersion = testRunSession.BuildVersion
            };

            _context.TestRunSessions.Add(entity);
            await _context.SaveChangesAsync();

            return await GetByIdAsync(entity.Id);
        }

        public async Task<bool> UpdateAsync(TestRunSessionDto testRunSession)
        {
            var entity = await _context.TestRunSessions.FindAsync(testRunSession.Id);
            if (entity == null)
                return false;

            entity.Name = testRunSession.Name;
            entity.Description = testRunSession.Description;
            entity.TestPlanId = testRunSession.TestPlanId;
            entity.ExecutedBy = testRunSession.ExecutedBy;
            entity.StartedAt = testRunSession.StartedAt;
            entity.CompletedAt = testRunSession.CompletedAt;
            entity.Status = testRunSession.Status;
            entity.Environment = testRunSession.Environment;
            entity.BuildVersion = testRunSession.BuildVersion;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _context.TestRunSessions.FindAsync(id);
            if (entity == null)
                return false;

            _context.TestRunSessions.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<TestRunSessionDto?> StartTestRunSessionAsync(TestRunSessionDto testRunSession)
        {
            // Set the session to InProgress and start time
            testRunSession.Status = TestRunStatus.InProgress;
            testRunSession.StartedAt = DateTime.UtcNow;
            testRunSession.CompletedAt = null;

            return await CreateAsync(testRunSession);
        }

        public async Task<bool> CompleteTestRunSessionAsync(int id)
        {
            var entity = await _context.TestRunSessions.FindAsync(id);
            if (entity == null)
                return false;

            entity.Status = TestRunStatus.Completed;
            entity.CompletedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AbortTestRunSessionAsync(int id)
        {
            var entity = await _context.TestRunSessions.FindAsync(id);
            if (entity == null)
                return false;

            entity.Status = TestRunStatus.Aborted;
            entity.CompletedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<TestRunSessionDto>> GetActiveSessionsAsync()
        {
            var sessions = await _context.TestRunSessions
                .Include(trs => trs.TestPlan)
                .Include(trs => trs.Executor)
                .Include(trs => trs.TestCaseExecutions)
                .Where(trs => trs.Status == TestRunStatus.InProgress || trs.Status == TestRunStatus.Paused)
                .OrderByDescending(trs => trs.StartedAt)
                .ToListAsync();

            return sessions.Select(MapToDto).ToList();
        }

        private static TestRunSessionDto MapToDto(TestRunSession entity)
        {
            return new TestRunSessionDto
            {
                Id = entity.Id,
                Name = entity.Name,
                Description = entity.Description,
                TestPlanId = entity.TestPlanId,
                ExecutedBy = entity.ExecutedBy,
                StartedAt = entity.StartedAt,
                CompletedAt = entity.CompletedAt,
                Status = entity.Status,
                Environment = entity.Environment,
                BuildVersion = entity.BuildVersion,
                TestPlanName = entity.TestPlan?.Name,
                ExecutorName = entity.Executor?.UserName,
                TestCaseExecutions = MapTestCaseExecutions(entity.TestCaseExecutions)
            };
        }

        private static List<TestCaseExecutionDto> MapTestCaseExecutions(ICollection<TestCaseExecution>? testCaseExecutions)
        {
            if (testCaseExecutions == null)
                return new List<TestCaseExecutionDto>();

            return testCaseExecutions.Select(MapTestCaseExecutionToDto).ToList();
        }

        private static TestCaseExecutionDto MapTestCaseExecutionToDto(TestCaseExecution tce)
        {
            return new TestCaseExecutionDto
            {
                Id = tce.Id,
                TestRunSessionId = tce.TestRunSessionId,
                TestCaseId = tce.TestCaseId,
                OverallResult = tce.OverallResult,
                ExecutedAt = tce.ExecutedAt,
                ExecutedBy = tce.ExecutedBy,
                Notes = tce.Notes,
                DefectId = tce.DefectId,
                TestCaseTitle = tce.TestCase?.Title,
                ExecutorName = tce.Executor?.UserName,
                TestStepExecutions = MapTestStepExecutions(tce.TestStepExecutions)
            };
        }

        private static List<TestStepExecutionDto> MapTestStepExecutions(ICollection<TestStepExecution>? testStepExecutions)
        {
            if (testStepExecutions == null)
                return new List<TestStepExecutionDto>();

            return testStepExecutions.Select(MapTestStepExecutionToDto).ToList();
        }

        private static TestStepExecutionDto MapTestStepExecutionToDto(TestStepExecution tse)
        {
            return new TestStepExecutionDto
            {
                Id = tse.Id,
                TestCaseExecutionId = tse.TestCaseExecutionId,
                TestStepId = tse.TestStepId,
                StepOrder = tse.StepOrder,
                Result = tse.Result,
                ActualResult = tse.ActualResult,
                Notes = tse.Notes,
                ExecutedAt = tse.ExecutedAt,
                StepDescription = tse.TestStep?.Description,
                ExpectedResult = tse.TestStep?.ExpectedResult
            };
        }
    }
}