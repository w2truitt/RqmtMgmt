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
        /// Retrieves all test cases from the database including their associated test steps and creator details.
        /// </summary>
        /// <returns>A list of all test cases as DTOs with their test steps and creator information.</returns>
        public async Task<List<TestCaseDto>> GetAllAsync()
        {
            var testCases = await _context.TestCases
                .Include(tc => tc.Steps)
                .Include(tc => tc.Creator)
                .Include(tc => tc.UpdatedByUser)
                .ToListAsync();
            return testCases.Select(ToDto).ToList();
        }

        /// <summary>
        /// Retrieves a specific test case by its ID including associated test steps and creator details.
        /// </summary>
        /// <param name="id">The unique identifier of the test case.</param>
        /// <returns>The test case DTO if found; otherwise, null.</returns>
        public async Task<TestCaseDto?> GetByIdAsync(int id)
        {
            var testCase = await _context.TestCases
                .Include(tc => tc.Steps)
                .Include(tc => tc.Creator)
                .Include(tc => tc.UpdatedByUser)
                .FirstOrDefaultAsync(tc => tc.Id == id);
            return testCase == null ? null : ToDto(testCase);
        }

        /// <summary>
        /// Creates a new test case with validation for required fields and test steps.
        /// </summary>
        /// <param name="testCase">The test case data to create.</param>
        /// <returns>The created test case DTO if successful; otherwise, null.</returns>
        public async Task<TestCaseDto?> CreateAsync(TestCaseDto testCase)
        {
            // Validate required fields
            if (string.IsNullOrWhiteSpace(testCase.Title))
                return null;

            if (testCase.CreatedBy <= 0)
                return null;

            // Validate steps - ensure all steps have required fields
            if (testCase.Steps != null)
            {
                foreach (var step in testCase.Steps)
                {
                    if (string.IsNullOrWhiteSpace(step.Description) || string.IsNullOrWhiteSpace(step.ExpectedResult))
                        return null;
                }
            }

            var entity = FromDto(testCase);
            entity.CreatedBy = testCase.CreatedBy;
            entity.CreatedAt = testCase.CreatedAt;
            _context.TestCases.Add(entity);
            await _context.SaveChangesAsync();
            
            // Reload with creator details
            var createdTestCase = await _context.TestCases
                .Include(tc => tc.Steps)
                .Include(tc => tc.Creator)
                .FirstOrDefaultAsync(tc => tc.Id == entity.Id);
            
            return createdTestCase == null ? null : ToDto(createdTestCase);
        }

        /// <summary>
        /// Updates an existing test case with validation and replaces all associated test steps.
        /// </summary>
        /// <param name="testCase">The test case data to update.</param>
        /// <returns>True if the update was successful; otherwise, false.</returns>
        public async Task<bool> UpdateAsync(TestCaseDto testCase)
        {
            var tracked = await _context.TestCases.Include(tc => tc.Steps).FirstOrDefaultAsync(tc => tc.Id == testCase.Id);
            if (tracked == null) return false;

            // Validate the DTO before proceeding
            if (!IsValidTestCaseDto(testCase))
                return false;

            // Update the entity properties
            UpdateTestCaseProperties(tracked, testCase);

            // Set update tracking fields
            tracked.UpdatedAt = DateTime.UtcNow;
            if (testCase.UpdatedBy?.Id > 0)
            {
                tracked.UpdatedBy = testCase.UpdatedBy.Id;
            }

            // Replace the test steps
            ReplaceTestSteps(tracked, testCase.Steps);

            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Validates a test case DTO for required fields and business rules.
        /// </summary>
        /// <param name="dto">The test case DTO to validate.</param>
        /// <returns>True if the DTO is valid; otherwise, false.</returns>
        private static bool IsValidTestCaseDto(TestCaseDto testCase)
        {
            if (string.IsNullOrWhiteSpace(testCase.Title))
                return false;

            if (testCase.CreatedBy <= 0)
                return false;

            return AreTestStepsValid(testCase.Steps);
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
        private static void UpdateTestCaseProperties(TestCase entity, TestCaseDto testCase)
        {
            entity.Title = testCase.Title;
            entity.Description = testCase.Description;
            entity.SuiteId = testCase.SuiteId;
            entity.Priority = testCase.Priority;
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
        /// Retrieves test cases with pagination, filtering, and sorting capabilities.
        /// </summary>
        /// <param name="parameters">Pagination parameters including page number, size, search term, and sorting options.</param>
        /// <returns>A paginated result containing test cases and pagination metadata.</returns>
        public async Task<PagedResult<TestCaseDto>> GetPagedAsync(PaginationParameters parameters)
        {
            var query = _context.TestCases
                .Include(tc => tc.Steps)
                .Include(tc => tc.Creator)
                .Include(tc => tc.Suite)
                .AsQueryable();

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(parameters.SearchTerm))
            {
                query = query.Where(tc => tc.Title.Contains(parameters.SearchTerm) || 
                                        (tc.Description != null && tc.Description.Contains(parameters.SearchTerm)));
            }

            // Apply test suite filter if specified
            if (parameters.SuiteId.HasValue)
            {
                query = query.Where(tc => tc.SuiteId == parameters.SuiteId.Value);
            }

            // Get total count for pagination metadata
            var totalItems = await query.CountAsync();

            // Apply pagination
            var testCases = await query
                .OrderBy(tc => tc.Title)
                .Skip((parameters.PageNumber - 1) * parameters.PageSize)
                .Take(parameters.PageSize)
                .ToListAsync();

            return new PagedResult<TestCaseDto>
            {
                Items = testCases.Select(ToDto).ToList(),
                PageNumber = parameters.PageNumber,
                PageSize = parameters.PageSize,
                TotalItems = totalItems
            };
        }

        /// <summary>
        /// Retrieves all test cases for a specific test suite.
        /// </summary>
        /// <param name="testSuiteId">The unique identifier of the test suite.</param>
        /// <returns>A list of test cases for the specified test suite.</returns>
        public async Task<List<TestCaseDto>> GetByTestSuiteIdAsync(int testSuiteId)
        {
            var testCases = await _context.TestCases
                .Include(tc => tc.Steps)
                .Include(tc => tc.Creator)
                .Include(tc => tc.Suite)
                .Where(tc => tc.SuiteId == testSuiteId)
                .ToListAsync();

            return testCases.Select(ToDto).ToList();
        }

        /// <summary>
        /// Retrieves test cases for a specific test suite with pagination, filtering, and sorting capabilities.
        /// </summary>
        /// <param name="testSuiteId">The unique identifier of the test suite.</param>
        /// <param name="parameters">Pagination parameters including page number, size, search term, and sorting options.</param>
        /// <returns>A paginated result containing test cases for the test suite and pagination metadata.</returns>
        public async Task<PagedResult<TestCaseDto>> GetPagedByTestSuiteIdAsync(int testSuiteId, PaginationParameters parameters)
        {
            var query = _context.TestCases
                .Include(tc => tc.Steps)
                .Include(tc => tc.Creator)
                .Include(tc => tc.Suite)
                .Where(tc => tc.SuiteId == testSuiteId);

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(parameters.SearchTerm))
            {
                query = query.Where(tc => tc.Title.Contains(parameters.SearchTerm) || 
                                        (tc.Description != null && tc.Description.Contains(parameters.SearchTerm)));
            }

            // Get total count for pagination metadata
            var totalItems = await query.CountAsync();

            // Apply pagination
            var testCases = await query
                .OrderBy(tc => tc.Title)
                .Skip((parameters.PageNumber - 1) * parameters.PageSize)
                .Take(parameters.PageSize)
                .ToListAsync();

            return new PagedResult<TestCaseDto>
            {
                Items = testCases.Select(ToDto).ToList(),
                PageNumber = parameters.PageNumber,
                PageSize = parameters.PageSize,
                TotalItems = totalItems
            };
        }

            /// <summary>
            /// Retrieves test cases for a specific project with pagination, filtering, and sorting capabilities.
            /// This includes test cases from all test suites belonging to the project, plus any unassigned test cases.
            /// </summary>
            /// <param name="projectId">The unique identifier of the project.</param>
            /// <param name="parameters">Pagination parameters including page number, size, search term, and sorting options.</param>
            /// <returns>A paginated result containing test cases for the project and pagination metadata.</returns>
            public async Task<PagedResult<TestCaseDto>> GetPagedByProjectIdAsync(int projectId, PaginationParameters parameters)
            {
                var query = _context.TestCases
                    .Include(tc => tc.Steps)
                    .Include(tc => tc.Creator)
                    .Include(tc => tc.Suite)
                    .Where(tc => tc.Suite == null || tc.Suite.ProjectId == projectId);

                // Apply search filter
                if (!string.IsNullOrWhiteSpace(parameters.SearchTerm))
                {
                    query = query.Where(tc => tc.Title.Contains(parameters.SearchTerm) || 
                                            (tc.Description != null && tc.Description.Contains(parameters.SearchTerm)));
                }

                // Get total count for pagination metadata
                var totalItems = await query.CountAsync();

                // Apply pagination
                var testCases = await query
                    .OrderBy(tc => tc.Title)
                    .Skip((parameters.PageNumber - 1) * parameters.PageSize)
                    .Take(parameters.PageSize)
                    .ToListAsync();

                return new PagedResult<TestCaseDto>
                {
                    Items = testCases.Select(ToDto).ToList(),
                    PageNumber = parameters.PageNumber,
                    PageSize = parameters.PageSize,
                        TotalItems = totalItems,
                };
            }

        /// <summary>
        /// Converts a TestCase entity to a TestCaseDto for API responses.
        /// Includes mapping of associated test steps and creator details.
        /// </summary>
        /// <param name="tc">The test case entity to convert.</param>
        /// <returns>A TestCaseDto with all properties, test steps, and creator details mapped.</returns>
        private static TestCaseDto ToDto(TestCase tc) => new TestCaseDto
        {
            Id = tc.Id,
            SuiteId = tc.SuiteId,
            Title = tc.Title,
            Description = tc.Description,
            Priority = tc.Priority,
            Steps = tc.Steps != null
                ? tc.Steps.Select(s => new TestStepDto
                {
                    Id = s.Id,
                    Description = s.Description,
                    ExpectedResult = s.ExpectedResult
                }).ToList()
                : new List<TestStepDto>(),
            CreatedBy = tc.CreatedBy,
            CreatedByUser = tc.Creator != null ? new UserDto
            {
                Id = tc.Creator.Id,
                UserName = tc.Creator.UserName,
                Email = tc.Creator.Email,
                Roles = new List<string>() // Roles are not loaded in this context for performance
            } : null,
            CreatedAt = tc.CreatedAt,
            UpdatedBy = tc.UpdatedByUser != null ? new UserDto
            {
                Id = tc.UpdatedByUser.Id,
                UserName = tc.UpdatedByUser.UserName,
                Email = tc.UpdatedByUser.Email,
                Roles = new List<string>() // Roles are not loaded in this context for performance
            } : null,
            UpdatedAt = tc.UpdatedAt
        };

        /// <summary>
        /// Converts a TestCaseDto to a TestCase entity for database operations.
        /// Creates associated TestStep entities from the DTO's steps collection.
        /// </summary>
        /// <param name="dto">The test case DTO to convert.</param>
        /// <returns>A TestCase entity with all properties and test steps mapped.</returns>
        private static TestCase FromDto(TestCaseDto testCaseDto)
        {
            var testCase = new TestCase
            {
                Id = testCaseDto.Id,
                SuiteId = testCaseDto.SuiteId,
                Title = testCaseDto.Title,
                Description = testCaseDto.Description,
                Priority = testCaseDto.Priority == 0 ? TestCasePriority.Medium : testCaseDto.Priority,
                CreatedBy = testCaseDto.CreatedBy,
                CreatedAt = testCaseDto.CreatedAt,
                UpdatedBy = testCaseDto.UpdatedBy?.Id,
                UpdatedAt = testCaseDto.UpdatedAt,
                Steps = new List<TestStep>()
            };
            if (testCaseDto.Steps != null)
            {
                foreach (var s in testCaseDto.Steps)
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
