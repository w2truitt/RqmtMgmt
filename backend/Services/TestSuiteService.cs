using backend.Data;
using backend.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace backend.Services
{
    public class TestSuiteService : ITestSuiteService
    {
        private readonly RqmtMgmtDbContext _context;
        public TestSuiteService(RqmtMgmtDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<TestSuite>> GetAllAsync()
        {
            return await _context.TestSuites.ToListAsync();
        }

        public async Task<TestSuite> GetByIdAsync(int id)
        {
            return await _context.TestSuites.FindAsync(id);
        }

        public async Task<TestSuite> CreateAsync(TestSuite suite)
        {
            _context.TestSuites.Add(suite);
            await _context.SaveChangesAsync();
            return suite;
        }

        public async Task<TestSuite> UpdateAsync(TestSuite suite)
        {
            _context.TestSuites.Update(suite);
            await _context.SaveChangesAsync();
            return suite;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var suite = await _context.TestSuites.FindAsync(id);
            if (suite == null) return false;
            _context.TestSuites.Remove(suite);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
