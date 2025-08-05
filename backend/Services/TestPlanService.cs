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

        public async Task<TestPlan> UpdateAsync(TestPlan updated)
        {
            var tracked = await _context.TestPlans.FindAsync(updated.Id);
            if (tracked == null)
                throw new KeyNotFoundException($"TestPlan with ID {updated.Id} not found.");

            tracked.Name = updated.Name;
            tracked.Type = updated.Type;
            tracked.Description = updated.Description;
            tracked.CreatedBy = updated.CreatedBy;
            tracked.CreatedAt = updated.CreatedAt;

            await _context.SaveChangesAsync();
            return tracked;
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
