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

            // Validate the DTO before proceeding
            if (!IsValidTestCaseDto(dto))
                return false;

            // Update the entity properties
            UpdateTestCaseProperties(tracked, dto);

            // Replace the test steps
            ReplaceTestSteps(tracked, dto.Steps);

            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Validates a test case DTO for required fields and business rules.
        /// </summary>
        /// <param name="dto">The test case DTO to validate.</param>
        /// <returns>True if the DTO is valid; otherwise, false.</returns>
        private static bool IsValidTestCaseDto(TestCaseDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Title))
                return false;

            if (dto.CreatedBy <= 0)
                return false;

            return AreTestStepsValid(dto.Steps);
        }

        /// <summary>
        /// Validates test steps to ensure all required fields are present.
        /// </summary>
        /// <param name="steps">The test steps to validate.</param>
        /// <returns>True if all steps are valid; otherwise, false.</returns>
        private static bool AreTestStepsValid(IEnumerable<TestStepDto>? steps)
        {
            if (steps == null) return true;

            return steps.All(step => 
                !string.IsNullOrWhiteSpace(step.Description) && 
                !string.IsNullOrWhiteSpace(step.ExpectedResult));
        }

        /// <summary>
        /// Updates the properties of a test case entity from a DTO.
        /// </summary>
        /// <param name="entity">The test case entity to update.</param>
        /// <param name="dto">The DTO containing the new values.</param>
        private static void UpdateTestCaseProperties(TestCase entity, TestCaseDto dto)
        {
            entity.Title = dto.Title;
            entity.Description = dto.Description;
            entity.SuiteId = dto.SuiteId;
        }

        /// <summary>
        /// Replaces all test steps in a test case with new ones from the DTO.
        /// </summary>
        /// <param name="entity">The test case entity to update.</param>
        /// <param name="steps">The new test steps from the DTO.</param>
        private static void ReplaceTestSteps(TestCase entity, IEnumerable<TestStepDto>? steps)
        {
            entity.Steps.Clear();
            
            if (steps == null) return;

            foreach (var stepDto in steps)
            {
                entity.Steps.Add(new TestStep 
                { 
                    Description = stepDto.Description, 
                    ExpectedResult = stepDto.ExpectedResult 
                });
            }
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