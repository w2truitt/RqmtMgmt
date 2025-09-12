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
        /// Validates input data before creation to ensure data integrity.
        /// </summary>
        /// <param name="testPlan">The test plan data to create.</param>
        /// <returns>The created test plan DTO if successful; otherwise, null.</returns>
        public async Task<TestPlanDto?> CreateAsync(TestPlanDto testPlan)
        {
            // Validate required fields
            if (string.IsNullOrWhiteSpace(testPlan.Name))
                return null;

            if (testPlan.CreatedBy <= 0)
                return null;

            // Validate test plan type
            if (string.IsNullOrWhiteSpace(testPlan.Type) || 
                !Enum.TryParse<TestPlanType>(testPlan.Type, out _))
                return null;

            try
            {
                var entity = FromDto(testPlan);
                _context.TestPlans.Add(entity);
                await _context.SaveChangesAsync();
                return ToDto(entity);
            }
            catch (Exception)
            {
                // Log the exception in a real application
                return null;
            }
        }

        /// <summary>
        /// Updates an existing test plan with new data including type conversion.
        /// Validates input data before update to ensure data integrity.
        /// </summary>
        /// <param name="testPlan">The test plan data to update.</param>
        /// <returns>True if the update was successful; otherwise, false.</returns>
        public async Task<bool> UpdateAsync(TestPlanDto testPlan)
        {
            var tracked = await _context.TestPlans.FindAsync(testPlan.Id);
            if (tracked == null) return false;
            
            // Validate required fields
            if (string.IsNullOrWhiteSpace(testPlan.Name))
                return false;

            if (testPlan.CreatedBy <= 0)
                return false;

            // Validate test plan type
            if (string.IsNullOrWhiteSpace(testPlan.Type) || 
                !Enum.TryParse<TestPlanType>(testPlan.Type, out var parsedType))
                return false;

            try
            {
                tracked.Name = testPlan.Name;
                tracked.Type = parsedType;
                tracked.Description = testPlan.Description;
                tracked.CreatedBy = testPlan.CreatedBy;
                tracked.CreatedAt = testPlan.CreatedAt;
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                // Log the exception in a real application
                return false;
            }
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
        /// Retrieves test plans with pagination, filtering, and sorting capabilities.
        /// </summary>
        /// <param name="parameters">Pagination parameters including page number, size, search term, and sorting options.</param>
        /// <returns>A paginated result containing test plans and pagination metadata.</returns>
        public async Task<PagedResult<TestPlanDto>> GetPagedAsync(PaginationParameters parameters)
        {
            var query = _context.TestPlans
                .Include(tp => tp.Project)
                .AsQueryable();

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(parameters.SearchTerm))
            {
                query = query.Where(tp => tp.Name.Contains(parameters.SearchTerm) || 
                                        (tp.Description != null && tp.Description.Contains(parameters.SearchTerm)));
            }

            // Apply project filter if specified
            if (parameters.ProjectId.HasValue)
            {
                query = query.Where(tp => tp.ProjectId == parameters.ProjectId.Value);
            }

            // Get total count for pagination metadata
            var totalItems = await query.CountAsync();

            // Apply pagination
            var plans = await query
                .OrderBy(tp => tp.Name)
                .Skip((parameters.PageNumber - 1) * parameters.PageSize)
                .Take(parameters.PageSize)
                .ToListAsync();

            return new PagedResult<TestPlanDto>
            {
                Items = plans.Select(ToDto).ToList(),
                PageNumber = parameters.PageNumber,
                PageSize = parameters.PageSize,
                TotalItems = totalItems
            };
        }

        /// <summary>
        /// Retrieves all test plans for a specific project.
        /// </summary>
        /// <param name="projectId">The unique identifier of the project.</param>
        /// <returns>A list of test plans for the specified project.</returns>
        public async Task<List<TestPlanDto>> GetByProjectIdAsync(int projectId)
        {
            var plans = await _context.TestPlans
                .Include(tp => tp.Project)
                .Where(tp => tp.ProjectId == projectId)
                .ToListAsync();

            return plans.Select(ToDto).ToList();
        }

        /// <summary>
        /// Retrieves test plans for a specific project with pagination, filtering, and sorting capabilities.
        /// </summary>
        /// <param name="projectId">The unique identifier of the project.</param>
        /// <param name="parameters">Pagination parameters including page number, size, search term, and sorting options.</param>
        /// <returns>A paginated result containing test plans for the project and pagination metadata.</returns>
        public async Task<PagedResult<TestPlanDto>> GetPagedByProjectIdAsync(int projectId, PaginationParameters parameters)
        {
            var query = _context.TestPlans
                .Include(tp => tp.Project)
                .Where(tp => tp.ProjectId == projectId);

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(parameters.SearchTerm))
            {
                query = query.Where(tp => tp.Name.Contains(parameters.SearchTerm) || 
                                        (tp.Description != null && tp.Description.Contains(parameters.SearchTerm)));
            }

            // Get total count for pagination metadata
            var totalItems = await query.CountAsync();

            // Apply pagination
            var plans = await query
                .OrderBy(tp => tp.Name)
                .Skip((parameters.PageNumber - 1) * parameters.PageSize)
                .Take(parameters.PageSize)
                .ToListAsync();

            return new PagedResult<TestPlanDto>
            {
                Items = plans.Select(ToDto).ToList(),
                PageNumber = parameters.PageNumber,
                PageSize = parameters.PageSize,
                TotalItems = totalItems
            };
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
            CreatedAt = tp.CreatedAt,
            ProjectId = tp.ProjectId
        };

        /// <summary>
        /// Converts a TestPlanDto to a TestPlan entity for database operations.
        /// Safely parses the string type to TestPlanType enum with fallback to UserValidation.
        /// </summary>
        /// <param name="dto">The test plan DTO to convert.</param>
        /// <returns>A TestPlan entity with all properties mapped and type converted from string.</returns>
        private static TestPlan FromDto(TestPlanDto testPlan) => new TestPlan
        {
            Id = testPlan.Id,
            Name = testPlan.Name,
            Type = Enum.TryParse<TestPlanType>(testPlan.Type, out var t) ? t : TestPlanType.UserValidation,
            Description = testPlan.Description,
            CreatedBy = testPlan.CreatedBy,
            CreatedAt = testPlan.CreatedAt,
            ProjectId = testPlan.ProjectId
        };
    }
}