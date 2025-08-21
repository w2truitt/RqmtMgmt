using backend.Data;
using RqmtMgmtShared;
using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace backend.Controllers
{
    /// <summary>
    /// API controller for version history and redline comparison of requirements and test cases.
    /// Provides endpoints for retrieving version histories and performing detailed field-by-field comparisons.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class RedlineController : ControllerBase
    {
        private readonly RqmtMgmtDbContext _db;
        private readonly RedlineService _redlineService;

        /// <summary>
        /// Initializes a new instance of the RedlineController with the specified database context.
        /// </summary>
        /// <param name="db">The database context for version and redline operations.</param>
        public RedlineController(RqmtMgmtDbContext db)
        {
            _db = db;
            _redlineService = new RedlineService();
        }

        /// <summary>
        /// Retrieves all versions of a specific requirement ordered by version number.
        /// Provides complete version history for requirement change tracking and audit purposes.
        /// </summary>
        /// <param name="requirementId">The unique identifier of the requirement.</param>
        /// <returns>A list of all requirement versions ordered by version number.</returns>
        /// <response code="200">Returns the list of requirement versions.</response>
        [HttpGet("requirement/{requirementId}/versions")]
        public async Task<ActionResult> GetRequirementVersions(int requirementId)
        {
            var versions = await _db.RequirementVersions
                .Where(v => v.RequirementId == requirementId)
                .OrderBy(v => v.Version)
                .ToListAsync();
            return Ok(versions);
        }

        /// <summary>
        /// Retrieves a specific requirement version by its version ID.
        /// </summary>
        /// <param name="versionId">The unique identifier of the requirement version.</param>
        /// <returns>The requirement version if found.</returns>
        /// <response code="200">Returns the requested requirement version.</response>
        /// <response code="404">If the requirement version is not found.</response>
        [HttpGet("requirement/version/{versionId}")]
        public async Task<ActionResult> GetRequirementVersion(int versionId)
        {
            var version = await _db.RequirementVersions.FindAsync(versionId);
            if (version == null) return NotFound();
            return Ok(version);
        }

        /// <summary>
        /// Performs a detailed redline comparison between two requirement versions.
        /// Analyzes field-by-field differences and categorizes changes as Added, Modified, or Removed.
        /// </summary>
        /// <param name="oldVersionId">The ID of the older requirement version to compare from.</param>
        /// <param name="newVersionId">The ID of the newer requirement version to compare to.</param>
        /// <returns>A RedlineResultDto containing detailed change information.</returns>
        /// <response code="200">Returns the redline comparison results.</response>
        /// <response code="404">If either requirement version is not found.</response>
        [HttpGet("requirement/redline/{oldVersionId}/{newVersionId}")]
        public async Task<ActionResult<RedlineResultDto>> RedlineRequirement(int oldVersionId, int newVersionId)
        {
            var oldV = await _db.RequirementVersions.FindAsync(oldVersionId);
            var newV = await _db.RequirementVersions.FindAsync(newVersionId);
            if (oldV == null || newV == null) return NotFound();
            
            var diff = _redlineService.CompareRequirements(oldV, newV);
            return Ok(diff);
        }

        /// <summary>
        /// Retrieves all versions of a specific test case ordered by version number.
        /// Provides complete version history for test case change tracking and audit purposes.
        /// </summary>
        /// <param name="testCaseId">The unique identifier of the test case.</param>
        /// <returns>A list of all test case versions ordered by version number.</returns>
        /// <response code="200">Returns the list of test case versions.</response>
        [HttpGet("testcase/{testCaseId}/versions")]
        public async Task<ActionResult> GetTestCaseVersions(int testCaseId)
        {
            var versions = await _db.TestCaseVersions
                .Where(v => v.TestCaseId == testCaseId)
                .OrderBy(v => v.Version)
                .ToListAsync();
            return Ok(versions);
        }

        /// <summary>
        /// Retrieves a specific test case version by its version ID.
        /// </summary>
        /// <param name="versionId">The unique identifier of the test case version.</param>
        /// <returns>The test case version if found.</returns>
        /// <response code="200">Returns the requested test case version.</response>
        /// <response code="404">If the test case version is not found.</response>
        [HttpGet("testcase/version/{versionId}")]
        public async Task<ActionResult> GetTestCaseVersion(int versionId)
        {
            var version = await _db.TestCaseVersions.FindAsync(versionId);
            if (version == null) return NotFound();
            return Ok(version);
        }

        /// <summary>
        /// Performs a detailed redline comparison between two test case versions.
        /// Analyzes field-by-field differences including title, description, steps, and expected results.
        /// </summary>
        /// <param name="oldVersionId">The ID of the older test case version to compare from.</param>
        /// <param name="newVersionId">The ID of the newer test case version to compare to.</param>
        /// <returns>A RedlineResultDto containing detailed change information.</returns>
        /// <response code="200">Returns the redline comparison results.</response>
        /// <response code="404">If either test case version is not found.</response>
        [HttpGet("testcase/redline/{oldVersionId}/{newVersionId}")]
        public async Task<ActionResult<RedlineResultDto>> RedlineTestCase(int oldVersionId, int newVersionId)
        {
            var oldV = await _db.TestCaseVersions.FindAsync(oldVersionId);
            var newV = await _db.TestCaseVersions.FindAsync(newVersionId);
            if (oldV == null || newV == null) return NotFound();
            
            var diff = _redlineService.CompareTestCases(oldV, newV);
            return Ok(diff);
        }
    }
}