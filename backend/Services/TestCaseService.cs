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
        private readonly RqmtMgmtDbContext _context;
        public TestCaseService(RqmtMgmtDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<TestCase>> GetAllAsync()
        {
            return await _context.TestCases.ToListAsync();
        }

        public async Task<TestCase?> GetByIdAsync(int id)
        {
            return await _context.TestCases.FindAsync(id);
        }

        public async Task<TestCase> CreateAsync(TestCase testCase)
        {
            _context.TestCases.Add(testCase);
            await _context.SaveChangesAsync();
            return testCase;
        }

        public async Task<TestCase> UpdateAsync(TestCase testCase)
        {
            _context.TestCases.Update(testCase);
            await _context.SaveChangesAsync();
            return testCase;
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
