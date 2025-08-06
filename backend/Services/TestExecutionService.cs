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
            var entity = new TestCaseExecution
            {
                TestRunSessionId = execution.TestRunSessionId,
                TestCaseId = execution.TestCaseId,
                OverallResult = execution.OverallResult,
                ExecutedAt = execution.ExecutedAt ?? DateTime.UtcNow,
                ExecutedBy = execution.ExecutedBy,
                Notes = execution.Notes,
                DefectId = execution.DefectId
            };

            _context.TestCaseExecutions.Add(entity);
            await _context.SaveChangesAsync();

            // If test step executions are provided, add them
            if (execution.TestStepExecutions?.Any() == true)
            {
                foreach (var stepExecution in execution.TestStepExecutions)
                {
                    var stepEntity = new TestStepExecution
                    {
                        TestCaseExecutionId = entity.Id,
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
            var stats = new TestExecutionStatsDto();

            // Get test run session statistics
            var testRunStats = await _context.TestRunSessions
                .GroupBy(trs => trs.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            stats.TotalTestRuns = testRunStats.Sum(s => s.Count);
            stats.ActiveTestRuns = testRunStats.FirstOrDefault(s => s.Status == TestRunStatus.InProgress)?.Count ?? 0;
            stats.CompletedTestRuns = testRunStats.FirstOrDefault(s => s.Status == TestRunStatus.Completed)?.Count ?? 0;

            // Get test case execution statistics
            var executionStats = await _context.TestCaseExecutions
                .GroupBy(tce => tce.OverallResult)
                .Select(g => new { Result = g.Key, Count = g.Count() })
                .ToListAsync();

            stats.TotalTestCaseExecutions = executionStats.Sum(s => s.Count);
            stats.PassedExecutions = executionStats.FirstOrDefault(s => s.Result == TestResult.Passed)?.Count ?? 0;
            stats.FailedExecutions = executionStats.FirstOrDefault(s => s.Result == TestResult.Failed)?.Count ?? 0;
            stats.BlockedExecutions = executionStats.FirstOrDefault(s => s.Result == TestResult.Blocked)?.Count ?? 0;
            stats.NotRunExecutions = executionStats.FirstOrDefault(s => s.Result == TestResult.NotRun)?.Count ?? 0;

            // Calculate pass rate
            if (stats.TotalTestCaseExecutions > 0)
            {
                stats.PassRate = (double)stats.PassedExecutions / stats.TotalTestCaseExecutions * 100;
            }

            // Get last execution date
            stats.LastExecutionDate = await _context.TestCaseExecutions
                .Where(tce => tce.ExecutedAt.HasValue)
                .MaxAsync(tce => (DateTime?)tce.ExecutedAt);

            return stats;
        }

        public async Task<TestExecutionStatsDto> GetExecutionStatsForSessionAsync(int testRunSessionId)
        {
            var stats = new TestExecutionStatsDto();

            // Get test case execution statistics for the specific session
            var executionStats = await _context.TestCaseExecutions
                .Where(tce => tce.TestRunSessionId == testRunSessionId)
                .GroupBy(tce => tce.OverallResult)
                .Select(g => new { Result = g.Key, Count = g.Count() })
                .ToListAsync();

            stats.TotalTestCaseExecutions = executionStats.Sum(s => s.Count);
            stats.PassedExecutions = executionStats.FirstOrDefault(s => s.Result == TestResult.Passed)?.Count ?? 0;
            stats.FailedExecutions = executionStats.FirstOrDefault(s => s.Result == TestResult.Failed)?.Count ?? 0;
            stats.BlockedExecutions = executionStats.FirstOrDefault(s => s.Result == TestResult.Blocked)?.Count ?? 0;
            stats.NotRunExecutions = executionStats.FirstOrDefault(s => s.Result == TestResult.NotRun)?.Count ?? 0;

            // Calculate pass rate
            if (stats.TotalTestCaseExecutions > 0)
            {
                stats.PassRate = (double)stats.PassedExecutions / stats.TotalTestCaseExecutions * 100;
            }

            // Get last execution date for this session
            stats.LastExecutionDate = await _context.TestCaseExecutions
                .Where(tce => tce.TestRunSessionId == testRunSessionId && tce.ExecutedAt.HasValue)
                .MaxAsync(tce => (DateTime?)tce.ExecutedAt);

            // For session-specific stats, set test run counts to 1 (this session)
            stats.TotalTestRuns = 1;
            stats.ActiveTestRuns = 0;
            stats.CompletedTestRuns = 0;

            return stats;
        }

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
}