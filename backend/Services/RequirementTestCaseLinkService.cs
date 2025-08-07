using backend.Data;
using RqmtMgmtShared;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace backend.Services
{
    /// <summary>
    /// Service implementation for managing requirement-to-test case traceability links.
    /// Provides operations to create, retrieve, and manage bidirectional links between requirements and test cases.
    /// </summary>
    public class RequirementTestCaseLinkService : IRequirementTestCaseLinkService
    {
        private readonly RqmtMgmtDbContext _db;

        /// <summary>
        /// Initializes a new instance of the RequirementTestCaseLinkService with the specified database context.
        /// </summary>
        /// <param name="db">The database context for link operations.</param>
        public RequirementTestCaseLinkService(RqmtMgmtDbContext db) => _db = db;

        /// <summary>
        /// Retrieves all test case links for a specific requirement.
        /// Provides traceability from requirements to their validating test cases.
        /// </summary>
        /// <param name="requirementId">The unique identifier of the requirement.</param>
        /// <returns>A list of links showing which test cases validate the specified requirement.</returns>
        public Task<List<RequirementTestCaseLinkDto>> GetLinksForRequirement(int requirementId) =>
            Task.FromResult(
                _db.RequirementTestCaseLinks
                    .Where(l => l.RequirementId == requirementId)
                    .Select(l => new RequirementTestCaseLinkDto { RequirementId = l.RequirementId, TestCaseId = l.TestCaseId })
                    .ToList());

        /// <summary>
        /// Retrieves all requirement links for a specific test case.
        /// Provides traceability from test cases to the requirements they validate.
        /// </summary>
        /// <param name="testCaseId">The unique identifier of the test case.</param>
        /// <returns>A list of links showing which requirements are validated by the specified test case.</returns>
        public Task<List<RequirementTestCaseLinkDto>> GetLinksForTestCase(int testCaseId) =>
            Task.FromResult(
                _db.RequirementTestCaseLinks
                    .Where(l => l.TestCaseId == testCaseId)
                    .Select(l => new RequirementTestCaseLinkDto { RequirementId = l.RequirementId, TestCaseId = l.TestCaseId })
                    .ToList());

        /// <summary>
        /// Creates a traceability link between a requirement and a test case.
        /// Prevents duplicate links by checking for existing relationships before creation.
        /// </summary>
        /// <param name="requirementId">The unique identifier of the requirement.</param>
        /// <param name="testCaseId">The unique identifier of the test case.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task AddLink(int requirementId, int testCaseId)
        {
            // Check for existing link to prevent duplicates
            var exists = _db.RequirementTestCaseLinks.Any(l => l.RequirementId == requirementId && l.TestCaseId == testCaseId);
            if (!exists)
            {
                _db.RequirementTestCaseLinks.Add(new backend.Models.RequirementTestCaseLink { RequirementId = requirementId, TestCaseId = testCaseId });
                await _db.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Removes a traceability link between a requirement and a test case.
        /// Safely handles cases where the link doesn't exist.
        /// </summary>
        /// <param name="requirementId">The unique identifier of the requirement.</param>
        /// <param name="testCaseId">The unique identifier of the test case.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task RemoveLink(int requirementId, int testCaseId)
        {
            var link = _db.RequirementTestCaseLinks.FirstOrDefault(l => l.RequirementId == requirementId && l.TestCaseId == testCaseId);
            if (link != null)
            {
                _db.RequirementTestCaseLinks.Remove(link);
                await _db.SaveChangesAsync();
            }
        }
    }
}