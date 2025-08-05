using backend.Data;
using backend.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using RqmtMgmtShared;

namespace backend.Services
{
    public class TestSuiteService : ITestSuiteService
    {
        private readonly RqmtMgmtDbContext _context;
        public TestSuiteService(RqmtMgmtDbContext context)
        {
            _context = context;
        }

        public async Task<List<TestSuiteDto>> GetAllAsync()
        {
            var suites = await _context.TestSuites.ToListAsync();
            return suites.Select(ToDto).ToList();
        }

        public async Task<TestSuiteDto?> GetByIdAsync(int id)
        {
            var suite = await _context.TestSuites.FindAsync(id);
            return suite == null ? null : ToDto(suite);
        }

        public async Task<TestSuiteDto?> CreateAsync(TestSuiteDto dto)
        {
            var entity = FromDto(dto);
            entity.CreatedBy = dto.CreatedBy;
            entity.CreatedAt = dto.CreatedAt;
            _context.TestSuites.Add(entity);
            await _context.SaveChangesAsync();
            return ToDto(entity);
        }

        public async Task<bool> UpdateAsync(TestSuiteDto dto)
        {
            var tracked = await _context.TestSuites.FindAsync(dto.Id);
            if (tracked == null) return false;
            tracked.Name = dto.Name;
            tracked.Description = dto.Description;
            tracked.CreatedBy = dto.CreatedBy;
            tracked.CreatedAt = dto.CreatedAt;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var suite = await _context.TestSuites.FindAsync(id);
            if (suite == null) return false;
            _context.TestSuites.Remove(suite);
            await _context.SaveChangesAsync();
            return true;
        }

        private static TestSuiteDto ToDto(TestSuite s) => new TestSuiteDto
        {
            Id = s.Id,
            Name = s.Name,
            Description = s.Description,
            CreatedBy = s.CreatedBy,
            CreatedAt = s.CreatedAt
        };

        private static TestSuite FromDto(TestSuiteDto dto) => new TestSuite
        {
            Id = dto.Id,
            Name = dto.Name,
            Description = dto.Description,
            CreatedBy = dto.CreatedBy,
            CreatedAt = dto.CreatedAt
        };
    }
}
