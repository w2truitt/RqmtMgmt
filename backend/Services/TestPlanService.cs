using backend.Data;
using backend.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using RqmtMgmtShared;

namespace backend.Services
{
    /// <summary>
    /// Service implementation for managing test plans using the database context.
    /// Provides CRUD operations for test plan documents that organize test cases for validation activities.
    /// </summary>
    public class TestPlanService : ITestPlanService
    {
        private readonly RqmtMgmtDbContext _context;

        /// <summary>
        /// Initializes a new instance of the TestPlanService with the specified database context.
        /// </summary>
        /// <param name="context">The database context for test plan operations.</param>
        public TestPlanService(RqmtMgmtDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Retrieves all test plans from the database.
        /// </summary>
        /// <returns>A list of all test plans as DTOs.</returns>
        public async Task<List<TestPlanDto>> GetAllAsync()
        {
            var plans = await _context.TestPlans.ToListAsync();
            return plans.Select(ToDto).ToList();
        }

        /// <summary>
        /// Retrieves a specific test plan by its ID.
        /// </summary>
        /// <param name="id">The unique identifier of the test plan.</param>
        /// <returns>The test plan DTO if found; otherwise, null.</returns>
        public async Task<TestPlanDto?> GetByIdAsync(int id)
        {
            var plan = await _context.TestPlans.FindAsync(id);
            return plan == null ? null : ToDto(plan);
        }

        /// <summary>
        /// Creates a new test plan with the provided data and enum type conversion.
        /// </summary>
        /// <param name="dto">The test plan data to create.</param>
        /// <returns>The created test plan DTO if successful; otherwise, null.</returns>
        public async Task<TestPlanDto?> CreateAsync(TestPlanDto dto)
        {
            var entity = FromDto(dto);
            _context.TestPlans.Add(entity);
            await _context.SaveChangesAsync();
            return ToDto(entity);
        }

        /// <summary>
        /// Updates an existing test plan with new data including type conversion.
        /// </summary>
        /// <param name="dto">The test plan data to update.</param>
        /// <returns>True if the update was successful; otherwise, false.</returns>
        public async Task<bool> UpdateAsync(TestPlanDto dto)
        {
            var tracked = await _context.TestPlans.FindAsync(dto.Id);
            if (tracked == null) return false;
            
            tracked.Name = dto.Name;
            // Safe enum parsing with fallback to current value
            tracked.Type = Enum.TryParse<TestPlanType>(dto.Type, out var t) ? t : tracked.Type;
            tracked.Description = dto.Description;
            tracked.CreatedBy = dto.CreatedBy;
            tracked.CreatedAt = dto.CreatedAt;
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Deletes a test plan by its ID. Associated test case links are cascade deleted.
        /// </summary>
        /// <param name="id">The unique identifier of the test plan to delete.</param>
        /// <returns>True if the deletion was successful; otherwise, false.</returns>
        public async Task<bool> DeleteAsync(int id)
        {
            var plan = await _context.TestPlans.FindAsync(id);
            if (plan == null) return false;
            _context.TestPlans.Remove(plan);
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Converts a TestPlan entity to a TestPlanDto for API responses.
        /// Converts the TestPlanType enum to string representation.
        /// </summary>
        /// <param name="tp">The test plan entity to convert.</param>
        /// <returns>A TestPlanDto with all properties mapped and enum converted to string.</returns>
        private static TestPlanDto ToDto(TestPlan tp) => new TestPlanDto
        {
            Id = tp.Id,
            Name = tp.Name,
            Type = tp.Type.ToString(),
            Description = tp.Description,
            CreatedBy = tp.CreatedBy,
            CreatedAt = tp.CreatedAt
        };

        /// <summary>
        /// Converts a TestPlanDto to a TestPlan entity for database operations.
        /// Safely parses the string type to TestPlanType enum with fallback to UserValidation.
        /// </summary>
        /// <param name="dto">The test plan DTO to convert.</param>
        /// <returns>A TestPlan entity with all properties mapped and type converted from string.</returns>
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