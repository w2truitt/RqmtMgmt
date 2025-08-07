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
    /// Provides CRUD operations with validation for test cases and their associated test steps.
    /// </summary>
    public class TestCaseService : ITestCaseService
    {
        private readonly RqmtMgmtDbContext _context;

        /// <summary>
        /// Initializes a new instance of the TestCaseService with the specified database context.
        /// </summary>
        /// <param name="context">The database context for test case operations.</param>
        public TestCaseService(RqmtMgmtDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Retrieves all test cases from the database including their associated test steps.
        /// </summary>
        /// <returns>A list of all test cases as DTOs with their test steps.</returns>
        public async Task<List<TestCaseDto>> GetAllAsync()
        {
            var testCases = await _context.TestCases.Include(tc => tc.Steps).ToListAsync();
            return testCases.Select(ToDto).ToList();
        }

        /// <summary>
        /// Retrieves a specific test case by its ID including associated test steps.
        /// </summary>
        /// <param name="id">The unique identifier of the test case.</param>
        /// <returns>The test case DTO if found; otherwise, null.</returns>
        public async Task<TestCaseDto?> GetByIdAsync(int id)
        {
            var testCase = await _context.TestCases.Include(tc => tc.Steps).FirstOrDefaultAsync(tc => tc.Id == id);
            return testCase == null ? null : ToDto(testCase);
        }

        /// <summary>
        /// Creates a new test case with validation for required fields and test steps.
        /// </summary>
        /// <param name="dto">The test case data to create.</param>
        /// <returns>The created test case DTO if successful; otherwise, null.</returns>
        public async Task<TestCaseDto?> CreateAsync(TestCaseDto dto)
        {
            // Validate required fields
            if (string.IsNullOrWhiteSpace(dto.Title))
                return null;

            if (dto.CreatedBy <= 0)
                return null;

            // Validate steps - ensure all steps have required fields
            if (dto.Steps != null)
            {
                foreach (var step in dto.Steps)
                {
                    if (string.IsNullOrWhiteSpace(step.Description) || string.IsNullOrWhiteSpace(step.ExpectedResult))
                        return null;
                }
            }

            var entity = FromDto(dto);
            entity.CreatedBy = dto.CreatedBy;
            entity.CreatedAt = dto.CreatedAt;
            _context.TestCases.Add(entity);
            await _context.SaveChangesAsync();
            return ToDto(entity);
        }

        /// <summary>
        /// Updates an existing test case with validation and replaces all associated test steps.
        /// </summary>
        /// <param name="dto">The test case data to update.</param>
        /// <returns>True if the update was successful; otherwise, false.</returns>
        public async Task<bool> UpdateAsync(TestCaseDto dto)
        {
            var tracked = await _context.TestCases.Include(tc => tc.Steps).FirstOrDefaultAsync(tc => tc.Id == dto.Id);
            if (tracked == null) return false;

            // Validate required fields
            if (string.IsNullOrWhiteSpace(dto.Title))
                return false;

            if (dto.CreatedBy <= 0)
                return false;

            // Validate steps - ensure all steps have required fields
            if (dto.Steps != null)
            {
                foreach (var step in dto.Steps)
                {
                    if (string.IsNullOrWhiteSpace(step.Description) || string.IsNullOrWhiteSpace(step.ExpectedResult))
                        return false;
                }
            }

            tracked.Title = dto.Title;
            tracked.Description = dto.Description;
            tracked.SuiteId = dto.SuiteId;
            // Replace steps (remove existing and add new ones)
            tracked.Steps.Clear();
            foreach (var s in dto.Steps ?? Enumerable.Empty<TestStepDto>())
            {
                tracked.Steps.Add(new TestStep { Description = s.Description, ExpectedResult = s.ExpectedResult });
            }
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Deletes a test case by its ID. Associated test steps are cascade deleted.
        /// </summary>
        /// <param name="id">The unique identifier of the test case to delete.</param>
        /// <returns>True if the deletion was successful; otherwise, false.</returns>
        public async Task<bool> DeleteAsync(int id)
        {
            var tc = await _context.TestCases.FindAsync(id);
            if (tc == null) return false;
            _context.TestCases.Remove(tc);
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Converts a TestCase entity to a TestCaseDto for API responses.
        /// Includes mapping of associated test steps.
        /// </summary>
        /// <param name="tc">The test case entity to convert.</param>
        /// <returns>A TestCaseDto with all properties and test steps mapped.</returns>
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

        /// <summary>
        /// Converts a TestCaseDto to a TestCase entity for database operations.
        /// Creates associated TestStep entities from the DTO's steps collection.
        /// </summary>
        /// <param name="dto">The test case DTO to convert.</param>
        /// <returns>A TestCase entity with all properties and test steps mapped.</returns>
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