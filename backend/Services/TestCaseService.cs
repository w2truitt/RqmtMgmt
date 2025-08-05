using backend.Data;
using backend.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using RqmtMgmtShared;

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

        public async Task<List<TestCaseDto>> GetAllAsync()
        {
            var testCases = await _context.TestCases.Include(tc => tc.Steps).ToListAsync();
            return testCases.Select(ToDto).ToList();
        }

        public async Task<TestCaseDto?> GetByIdAsync(int id)
        {
            var testCase = await _context.TestCases.Include(tc => tc.Steps).FirstOrDefaultAsync(tc => tc.Id == id);
            return testCase == null ? null : ToDto(testCase);
        }

        public async Task<TestCaseDto?> CreateAsync(TestCaseDto dto)
        {
            var entity = FromDto(dto);
            entity.CreatedBy = dto.CreatedBy;
            entity.CreatedAt = dto.CreatedAt;
            _context.TestCases.Add(entity);
            await _context.SaveChangesAsync();
            return ToDto(entity);
        }

        public async Task<bool> UpdateAsync(TestCaseDto dto)
        {
            var tracked = await _context.TestCases.Include(tc => tc.Steps).FirstOrDefaultAsync(tc => tc.Id == dto.Id);
            if (tracked == null) return false;
            tracked.Title = dto.Title;
            tracked.Description = dto.Description;
            tracked.SuiteId = dto.SuiteId;
            // Replace steps (remove and add)
            tracked.Steps.Clear();
            foreach (var s in dto.Steps)
            {
                tracked.Steps.Add(new TestStep { Description = s.Description, ExpectedResult = s.ExpectedResult });
            }
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var tc = await _context.TestCases.FindAsync(id);
            if (tc == null) return false;
            _context.TestCases.Remove(tc);
            await _context.SaveChangesAsync();
            return true;
        }

        // Mapping helpers
        private static TestCaseDto ToDto(TestCase tc) => new TestCaseDto
        {
            Id = tc.Id,
            SuiteId = tc.SuiteId,
            Title = tc.Title,
            Description = tc.Description,
            Steps = tc.Steps != null
                ? tc.Steps.Select(s => new TestStepDto
                {
                    Id = s.Id,
                    Description = s.Description,
                    ExpectedResult = s.ExpectedResult
                }).ToList()
                : new List<TestStepDto>(),
            CreatedBy = tc.CreatedBy,
            CreatedAt = tc.CreatedAt
        };

        private static TestCase FromDto(TestCaseDto dto)
        {
            var testCase = new TestCase
            {
                Id = dto.Id,
                SuiteId = dto.SuiteId,
                Title = dto.Title,
                Description = dto.Description,
                CreatedBy = dto.CreatedBy,
                CreatedAt = dto.CreatedAt,
                Steps = new List<TestStep>()
            };
            if (dto.Steps != null)
            {
                foreach (var s in dto.Steps)
                {
                    var step = new TestStep
                    {
                        Description = s.Description,
                        ExpectedResult = s.ExpectedResult,
                        TestCase = testCase
                    };
                    testCase.Steps.Add(step);
                }
            }
            return testCase;
        }
    }
}
