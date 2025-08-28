using backend.Data;
using backend.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using RqmtMgmtShared;

namespace backend.Services
{
    /// <summary>
    /// Service implementation for managing test suites using the database context.
    /// Provides CRUD operations for organizing test cases into logical groups.
    /// </summary>
    public class TestSuiteService : ITestSuiteService
    {
        private readonly RqmtMgmtDbContext _context;

        /// <summary>
        /// Initializes a new instance of the TestSuiteService with the specified database context.
        /// </summary>
        /// <param name="context">The database context for test suite operations.</param>
        public TestSuiteService(RqmtMgmtDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Retrieves all test suites from the database.
        /// </summary>
        /// <returns>A list of all test suites as DTOs.</returns>
        public async Task<List<TestSuiteDto>> GetAllAsync()
        {
            var suites = await _context.TestSuites.ToListAsync();
            return suites.Select(ToDto).ToList();
        }

        /// <summary>
        /// Retrieves a specific test suite by its ID.
        /// </summary>
        /// <param name="id">The unique identifier of the test suite.</param>
        /// <returns>The test suite DTO if found; otherwise, null.</returns>
        public async Task<TestSuiteDto?> GetByIdAsync(int id)
        {
            var suite = await _context.TestSuites.FindAsync(id);
            return suite == null ? null : ToDto(suite);
        }

        /// <summary>
        /// Creates a new test suite with the provided data.
        /// </summary>
        /// <param name="testSuite">The test suite data to create.</param>
        /// <returns>The created test suite DTO if successful; otherwise, null.</returns>
        public async Task<TestSuiteDto?> CreateAsync(TestSuiteDto testSuite)
        {
            // Validate required fields
            if (string.IsNullOrWhiteSpace(testSuite.Name))
                return null;

            if (testSuite.CreatedBy <= 0)
                return null;

            var entity = FromDto(testSuite);
            entity.CreatedBy = testSuite.CreatedBy;
            entity.CreatedAt = testSuite.CreatedAt;
            _context.TestSuites.Add(entity);
            await _context.SaveChangesAsync();
            
                // Reload the entity with navigation properties to ensure DTO is properly populated
                var createdEntity = await _context.TestSuites
                    .Include(ts => ts.Project)
                    .Include(ts => ts.TestCases)
                    .FirstOrDefaultAsync(ts => ts.Id == entity.Id);
            
                return createdEntity == null ? null : ToDto(createdEntity);
        }

        /// <summary>
        /// Updates an existing test suite with new data.
        /// </summary>
        /// <param name="testSuite">The test suite data to update.</param>
        /// <returns>True if the update was successful; otherwise, false.</returns>
        public async Task<bool> UpdateAsync(TestSuiteDto testSuite)
        {
            var tracked = await _context.TestSuites.FindAsync(testSuite.Id);
            if (tracked == null) return false;
            
            tracked.Name = testSuite.Name;
            tracked.Description = testSuite.Description;
            tracked.CreatedBy = testSuite.CreatedBy;
            tracked.CreatedAt = testSuite.CreatedAt;
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Deletes a test suite by its ID. Associated test cases will have their SuiteId set to null.
        /// </summary>
        /// <param name="id">The unique identifier of the test suite to delete.</param>
        /// <returns>True if the deletion was successful; otherwise, false.</returns>
        public async Task<bool> DeleteAsync(int id)
        {
            var suite = await _context.TestSuites.FindAsync(id);
            if (suite == null) return false;
            _context.TestSuites.Remove(suite);
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Converts a TestSuite entity to a TestSuiteDto for API responses.
        /// </summary>
        /// <param name="s">The test suite entity to convert.</param>
        /// <returns>A TestSuiteDto with all properties mapped.</returns>
        private static TestSuiteDto ToDto(TestSuite s) => new TestSuiteDto
        {
            Id = s.Id,
            Name = s.Name,
            Description = s.Description,
            CreatedBy = s.CreatedBy,
            CreatedAt = s.CreatedAt,
                ProjectId = s.ProjectId,
                ProjectName = s.Project?.Name ?? string.Empty,
                TestCaseCount = s.TestCases?.Count ?? 0
        };

        /// <summary>
        /// Converts a TestSuiteDto to a TestSuite entity for database operations.
        /// </summary>
        /// <param name="dto">The test suite DTO to convert.</param>
        /// <returns>A TestSuite entity with all properties mapped.</returns>
        private static TestSuite FromDto(TestSuiteDto testSuite) => new TestSuite
        {
            Id = testSuite.Id,
            Name = testSuite.Name,
            Description = testSuite.Description,
            CreatedBy = testSuite.CreatedBy,
            CreatedAt = testSuite.CreatedAt,
            ProjectId = testSuite.ProjectId
        };
    }
}