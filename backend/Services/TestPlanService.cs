using backend.Data;
using backend.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using RqmtMgmtShared;

namespace backend.Services
{
    public class TestPlanService : ITestPlanService
    {
        private readonly RqmtMgmtDbContext _context;
        public TestPlanService(RqmtMgmtDbContext context)
        {
            _context = context;
        }

        public async Task<List<TestPlanDto>> GetAllAsync()
        {
            var plans = await _context.TestPlans.ToListAsync();
            return plans.Select(ToDto).ToList();
        }

        public async Task<TestPlanDto?> GetByIdAsync(int id)
        {
            var plan = await _context.TestPlans.FindAsync(id);
            return plan == null ? null : ToDto(plan);
        }

        public async Task<TestPlanDto?> CreateAsync(TestPlanDto dto)
        {
            var entity = FromDto(dto);
            _context.TestPlans.Add(entity);
            await _context.SaveChangesAsync();
            return ToDto(entity);
        }

        public async Task<bool> UpdateAsync(TestPlanDto dto)
        {
            var tracked = await _context.TestPlans.FindAsync(dto.Id);
            if (tracked == null) return false;
            tracked.Name = dto.Name;
            tracked.Type = Enum.TryParse<TestPlanType>(dto.Type, out var t) ? t : tracked.Type;
            tracked.Description = dto.Description;
            tracked.CreatedBy = dto.CreatedBy;
            tracked.CreatedAt = dto.CreatedAt;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var plan = await _context.TestPlans.FindAsync(id);
            if (plan == null) return false;
            _context.TestPlans.Remove(plan);
            await _context.SaveChangesAsync();
            return true;
        }

        private static TestPlanDto ToDto(TestPlan tp) => new TestPlanDto
        {
            Id = tp.Id,
            Name = tp.Name,
            Type = tp.Type.ToString(),
            Description = tp.Description,
            CreatedBy = tp.CreatedBy,
            CreatedAt = tp.CreatedAt
        };

        private static TestPlan FromDto(TestPlanDto dto) => new TestPlan
        {
            Id = dto.Id,
            Name = dto.Name,
            Type = Enum.TryParse<TestPlanType>(dto.Type, out var t) ? t : TestPlanType.UserValidation,
            Description = dto.Description,
            CreatedBy = dto.CreatedBy,
            CreatedAt = dto.CreatedAt
        };
    }
}
