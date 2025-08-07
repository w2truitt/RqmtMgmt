using Microsoft.EntityFrameworkCore;
using RqmtMgmtShared;
using backend.Data;
using backend.Models;

namespace backend.Services
{
    /// <summary>
    /// Service for test execution and results tracking
    /// </summary>
    public class TestExecutionService : ITestExecutionService
    {
        private readonly RqmtMgmtDbContext _context;

        public TestExecutionService(RqmtMgmtDbContext context)
        {
            _context = context;
        }

        public async Task<TestCaseExecutionDto?> ExecuteTestCaseAsync(TestCaseExecutionDto execution)
        {
            var entity = CreateTestCaseExecutionEntity(execution);
            _context.TestCaseExecutions.Add(entity);
            await _context.SaveChangesAsync();

            await AddTestStepExecutionsIfProvidedAsync(execution, entity.Id);
            return await GetTestCaseExecutionByIdAsync(entity.Id);
        }

        public async Task<bool> UpdateTestCaseExecutionAsync(TestCaseExecutionDto execution)
        {
            var entity = await _context.TestCaseExecutions.FindAsync(execution.Id);
            if (entity == null)
                return false;

            entity.OverallResult = execution.OverallResult;
            entity.ExecutedAt = execution.ExecutedAt;
            entity.ExecutedBy = execution.ExecutedBy;
            entity.Notes = execution.Notes;
            entity.DefectId = execution.DefectId;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<TestStepExecutionDto?> UpdateStepResultAsync(TestStepExecutionDto stepExecution)
        {
            var entity = await _context.TestStepExecutions.FindAsync(stepExecution.Id);
            if (entity == null)
            {
                // If it doesn't exist, create a new one
                entity = new TestStepExecution
                {
                    TestCaseExecutionId = stepExecution.TestCaseExecutionId,
                    TestStepId = stepExecution.TestStepId,
                    StepOrder = stepExecution.StepOrder,
                    Result = stepExecution.Result,
                    ActualResult = stepExecution.ActualResult,
                    Notes = stepExecution.Notes,
                    ExecutedAt = stepExecution.ExecutedAt ?? DateTime.UtcNow
                };

                _context.TestStepExecutions.Add(entity);
            }
            else
            {
                // Update existing
                entity.Result = stepExecution.Result;
                entity.ActualResult = stepExecution.ActualResult;
                entity.Notes = stepExecution.Notes;
                entity.ExecutedAt = stepExecution.ExecutedAt ?? DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            return await GetTestStepExecutionByIdAsync(entity.Id);
        }

        public async Task<List<TestCaseExecutionDto>> GetExecutionsForSessionAsync(int testRunSessionId)
        {
            var executions = await _context.TestCaseExecutions
                .Include(tce => tce.TestCase)
                .Include(tce => tce.Executor)
                .Include(tce => tce.TestStepExecutions)
                    .ThenInclude(tse => tse.TestStep)
                .Where(tce => tce.TestRunSessionId == testRunSessionId)
                .OrderBy(tce => tce.TestCase!.Title)
                .ToListAsync();

            return executions.Select(MapTestCaseExecutionToDto).ToList();
        }

        public async Task<List<TestStepExecutionDto>> GetStepExecutionsForCaseAsync(int testCaseExecutionId)
        {
            var stepExecutions = await _context.TestStepExecutions
                .Include(tse => tse.TestStep)
                .Where(tse => tse.TestCaseExecutionId == testCaseExecutionId)
                .OrderBy(tse => tse.StepOrder)
                .ToListAsync();

            return stepExecutions.Select(MapTestStepExecutionToDto).ToList();
        }

        public async Task<TestExecutionStatsDto> GetExecutionStatsAsync()
        {
            var testRunStats = await GetTestRunStatsAsync();
            var executionStats = await GetTestCaseExecutionStatsAsync();
            var lastExecutionDate = await GetLastExecutionDateAsync();

            return BuildExecutionStats(testRunStats, executionStats, lastExecutionDate);
        }

        public async Task<TestExecutionStatsDto> GetExecutionStatsForSessionAsync(int testRunSessionId)
        {
            var executionStats = await GetTestCaseExecutionStatsForSessionAsync(testRunSessionId);
            var lastExecutionDate = await GetLastExecutionDateForSessionAsync(testRunSessionId);

            return BuildSessionExecutionStats(executionStats, lastExecutionDate);
        }

        #region Private Helper Methods

        private static TestCaseExecution CreateTestCaseExecutionEntity(TestCaseExecutionDto execution)
        {
            return new TestCaseExecution
            {
                TestRunSessionId = execution.TestRunSessionId,
                TestCaseId = execution.TestCaseId,
                OverallResult = execution.OverallResult,
                ExecutedAt = execution.ExecutedAt ?? DateTime.UtcNow,
                ExecutedBy = execution.ExecutedBy,
                Notes = execution.Notes,
                DefectId = execution.DefectId
            };
        }

        private async Task AddTestStepExecutionsIfProvidedAsync(TestCaseExecutionDto execution, int testCaseExecutionId)
        {
            if (execution.TestStepExecutions?.Any() != true)
                return;

            foreach (var stepExecution in execution.TestStepExecutions)
            {
                var stepEntity = new TestStepExecution
                {
                    TestCaseExecutionId = testCaseExecutionId,
                    TestStepId = stepExecution.TestStepId,
                    StepOrder = stepExecution.StepOrder,
                    Result = stepExecution.Result,
                    ActualResult = stepExecution.ActualResult,
                    Notes = stepExecution.Notes,
                    ExecutedAt = stepExecution.ExecutedAt ?? DateTime.UtcNow
                };

                _context.TestStepExecutions.Add(stepEntity);
            }

            await _context.SaveChangesAsync();
        }

        private async Task<List<TestRunStatusCount>> GetTestRunStatsAsync()
        {
            return await _context.TestRunSessions
                .GroupBy(trs => trs.Status)
                .Select(g => new TestRunStatusCount { Status = g.Key, Count = g.Count() })
                .ToListAsync();
        }

        private async Task<List<TestResultCount>> GetTestCaseExecutionStatsAsync()
        {
            return await _context.TestCaseExecutions
                .GroupBy(tce => tce.OverallResult)
                .Select(g => new TestResultCount { Result = g.Key, Count = g.Count() })
                .ToListAsync();
        }

        private async Task<List<TestResultCount>> GetTestCaseExecutionStatsForSessionAsync(int testRunSessionId)
        {
            return await _context.TestCaseExecutions
                .Where(tce => tce.TestRunSessionId == testRunSessionId)
                .GroupBy(tce => tce.OverallResult)
                .Select(g => new TestResultCount { Result = g.Key, Count = g.Count() })
                .ToListAsync();
        }

        private async Task<DateTime?> GetLastExecutionDateAsync()
        {
            return await _context.TestCaseExecutions
                .Where(tce => tce.ExecutedAt.HasValue)
                .MaxAsync(tce => (DateTime?)tce.ExecutedAt);
        }

        private async Task<DateTime?> GetLastExecutionDateForSessionAsync(int testRunSessionId)
        {
            return await _context.TestCaseExecutions
                .Where(tce => tce.TestRunSessionId == testRunSessionId && tce.ExecutedAt.HasValue)
                .MaxAsync(tce => (DateTime?)tce.ExecutedAt);
        }

        private static TestExecutionStatsDto BuildExecutionStats(
            List<TestRunStatusCount> testRunStats, 
            List<TestResultCount> executionStats, 
            DateTime? lastExecutionDate)
        {
            var stats = new TestExecutionStatsDto();

            // Set test run statistics
            stats.TotalTestRuns = testRunStats.Sum(s => s.Count);
            stats.ActiveTestRuns = GetCountByStatus(testRunStats, TestRunStatus.InProgress);
            stats.CompletedTestRuns = GetCountByStatus(testRunStats, TestRunStatus.Completed);

            // Set test case execution statistics
            PopulateTestCaseExecutionStats(stats, executionStats);
            stats.LastExecutionDate = lastExecutionDate;

            return stats;
        }

        private static TestExecutionStatsDto BuildSessionExecutionStats(
            List<TestResultCount> executionStats, 
            DateTime? lastExecutionDate)
        {
            var stats = new TestExecutionStatsDto();

            // For session-specific stats, set test run counts to 1 (this session)
            stats.TotalTestRuns = 1;
            stats.ActiveTestRuns = 0;
            stats.CompletedTestRuns = 0;

            // Set test case execution statistics
            PopulateTestCaseExecutionStats(stats, executionStats);
            stats.LastExecutionDate = lastExecutionDate;

            return stats;
        }

        private static void PopulateTestCaseExecutionStats(TestExecutionStatsDto stats, List<TestResultCount> executionStats)
        {
            stats.TotalTestCaseExecutions = executionStats.Sum(s => s.Count);
            stats.PassedExecutions = GetCountByResult(executionStats, TestResult.Passed);
            stats.FailedExecutions = GetCountByResult(executionStats, TestResult.Failed);
            stats.BlockedExecutions = GetCountByResult(executionStats, TestResult.Blocked);
            stats.NotRunExecutions = GetCountByResult(executionStats, TestResult.NotRun);

            // Calculate pass rate
            if (stats.TotalTestCaseExecutions > 0)
            {
                stats.PassRate = (double)stats.PassedExecutions / stats.TotalTestCaseExecutions * 100;
            }
        }

        private static int GetCountByStatus(List<TestRunStatusCount> stats, TestRunStatus status)
        {
            return stats.FirstOrDefault(s => s.Status == status)?.Count ?? 0;
        }

        private static int GetCountByResult(List<TestResultCount> stats, TestResult result)
        {
            return stats.FirstOrDefault(s => s.Result == result)?.Count ?? 0;
        }

        #endregion

        private async Task<TestCaseExecutionDto?> GetTestCaseExecutionByIdAsync(int id)
        {
            var execution = await _context.TestCaseExecutions
                .Include(tce => tce.TestCase)
                .Include(tce => tce.Executor)
                .Include(tce => tce.TestStepExecutions)
                    .ThenInclude(tse => tse.TestStep)
                .FirstOrDefaultAsync(tce => tce.Id == id);

            return execution != null ? MapTestCaseExecutionToDto(execution) : null;
        }

        private async Task<TestStepExecutionDto?> GetTestStepExecutionByIdAsync(int id)
        {
            var stepExecution = await _context.TestStepExecutions
                .Include(tse => tse.TestStep)
                .FirstOrDefaultAsync(tse => tse.Id == id);

            return stepExecution != null ? MapTestStepExecutionToDto(stepExecution) : null;
        }

        private static TestCaseExecutionDto MapTestCaseExecutionToDto(TestCaseExecution entity)
        {
            return new TestCaseExecutionDto
            {
                Id = entity.Id,
                TestRunSessionId = entity.TestRunSessionId,
                TestCaseId = entity.TestCaseId,
                OverallResult = entity.OverallResult,
                ExecutedAt = entity.ExecutedAt,
                ExecutedBy = entity.ExecutedBy,
                Notes = entity.Notes,
                DefectId = entity.DefectId,
                TestCaseTitle = entity.TestCase?.Title,
                ExecutorName = entity.Executor?.UserName,
                TestStepExecutions = entity.TestStepExecutions?.Select(MapTestStepExecutionToDto).ToList() ?? new List<TestStepExecutionDto>()
            };
        }

        private static TestStepExecutionDto MapTestStepExecutionToDto(TestStepExecution entity)
        {
            return new TestStepExecutionDto
            {
                Id = entity.Id,
                TestCaseExecutionId = entity.TestCaseExecutionId,
                TestStepId = entity.TestStepId,
                StepOrder = entity.StepOrder,
                Result = entity.Result,
                ActualResult = entity.ActualResult,
                Notes = entity.Notes,
                ExecutedAt = entity.ExecutedAt,
                StepDescription = entity.TestStep?.Description,
                ExpectedResult = entity.TestStep?.ExpectedResult
            };
        }
    }

    /// <summary>
    /// Helper class for grouping test run sessions by status with their counts.
    /// Used internally for statistical aggregation operations.
    /// </summary>
    public class TestRunStatusCount
    {
        /// <summary>
        /// Gets or sets the test run session status.
        /// </summary>
        public TestRunStatus Status { get; set; }
        
        /// <summary>
        /// Gets or sets the count of test run sessions with this status.
        /// </summary>
        public int Count { get; set; }
    }

    /// <summary>
    /// Helper class for grouping test case executions by result with their counts.
    /// Used internally for statistical aggregation operations.
    /// </summary>
    public class TestResultCount
    {
        /// <summary>
        /// Gets or sets the test case execution result.
        /// </summary>
        public TestResult Result { get; set; }
        
        /// <summary>
        /// Gets or sets the count of test case executions with this result.
        /// </summary>
        public int Count { get; set; }
    }
}