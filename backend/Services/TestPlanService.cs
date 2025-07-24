using backend.Data;
using backend.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace backend.Services
{
    public class TestPlanService : ITestPlanService
    {
        private readonly RqmtMgmtDbContext _context;
        public TestPlanService(RqmtMgmtDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<TestPlan>> GetAllAsync()
        {
            return await _context.TestPlans.ToListAsync();
        }

        public async Task<TestPlan?> GetByIdAsync(int id)
        {
            return await _context.TestPlans.FindAsync(id);
        }

        public async Task<TestPlan> CreateAsync(TestPlan plan)
        {
            _context.TestPlans.Add(plan);
            await _context.SaveChangesAsync();
            return plan;
        }

        public async Task<TestPlan> UpdateAsync(TestPlan plan)
        {
            _context.TestPlans.Update(plan);
            await _context.SaveChangesAsync();
            return plan;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var plan = await _context.TestPlans.FindAsync(id);
            if (plan == null) return false;
            _context.TestPlans.Remove(plan);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
