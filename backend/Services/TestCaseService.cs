using backend.Data;
using backend.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace backend.Services
{
    /// <summary>
    /// Service implementation for managing test cases using the database context.
    /// </summary>
    public class TestCaseService : ITestCaseService
    {
        public async Task<TestStep> AddStepAsync(int testCaseId, TestStep step)
        {
            var testCase = await _context.TestCases.Include(tc => tc.Steps).FirstOrDefaultAsync(tc => tc.Id == testCaseId);
            if (testCase == null) throw new KeyNotFoundException("Test case not found");
            // Determine step order (append to end)
            int order = testCase.Steps.Count > 0 ? testCase.Steps.Max(s => s.Id) + 1 : 1;
            step.TestCaseId = testCaseId;
            // step.Order = order; // Uncomment if using explicit ordering
            _context.TestSteps.Add(step);
            await _context.SaveChangesAsync();
            return step;
        }

        public async Task<bool> RemoveStepAsync(int testCaseId, int stepId)
        {
            var step = await _context.TestSteps.FirstOrDefaultAsync(s => s.Id == stepId && s.TestCaseId == testCaseId);
            if (step == null) return false;
            _context.TestSteps.Remove(step);
            await _context.SaveChangesAsync();
            return true;
        }

        private readonly RqmtMgmtDbContext _context;
        public TestCaseService(RqmtMgmtDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<TestCase>> GetAllAsync()
        {
            return await _context.TestCases.Include(tc => tc.Steps).ToListAsync();
        }

        public async Task<TestCase?> GetByIdAsync(int id)
        {
            return await _context.TestCases.Include(tc => tc.Steps).FirstOrDefaultAsync(tc => tc.Id == id);
        }

        public async Task<TestCase> CreateAsync(TestCase testCase)
        {
            // Always set CreatedBy to test user ID 1 until auth is enabled
            testCase.CreatedBy = 1;
            _context.TestCases.Add(testCase);
            await _context.SaveChangesAsync();
            return testCase;
        }

        public async Task<TestCase> UpdateAsync(TestCase updated)
        {
            var tracked = await _context.TestCases.Include(tc => tc.Steps).FirstOrDefaultAsync(tc => tc.Id == updated.Id);
            if (tracked == null)
                throw new KeyNotFoundException($"TestCase with ID {updated.Id} not found.");

            // Update only editable properties; preserve CreatedBy and CreatedAt
            tracked.Title = updated.Title;
            tracked.Description = updated.Description;
            tracked.SuiteId = updated.SuiteId;
            // tracked.CreatedBy and tracked.CreatedAt are intentionally NOT changed during update

            // Replace steps (for simplicity, remove and add)
            tracked.Steps.Clear();
            foreach (var step in updated.Steps)
            {
                tracked.Steps.Add(new TestStep {
                    Description = step.Description,
                    ExpectedResult = step.ExpectedResult
                });
            }

            await _context.SaveChangesAsync();
            return tracked;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var tc = await _context.TestCases.FindAsync(id);
            if (tc == null) return false;
            _context.TestCases.Remove(tc);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
