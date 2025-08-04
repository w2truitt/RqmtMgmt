using backend.Data;
using RqmtMgmtShared;
using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace backend.Controllers
{
    /// <summary>
    /// API endpoints for version history and redline comparison of requirements and test cases.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class RedlineController : ControllerBase
    {
        private readonly RqmtMgmtDbContext _db;
        private readonly RedlineService _redlineService;

        public RedlineController(RqmtMgmtDbContext db)
        {
            _db = db;
            _redlineService = new RedlineService();
        }

        // --- Requirement Version Endpoints ---

        [HttpGet("requirement/{requirementId}/versions")]
        public async Task<ActionResult> GetRequirementVersions(int requirementId)
        {
            var versions = await _db.RequirementVersions
                .Where(v => v.RequirementId == requirementId)
                .OrderBy(v => v.Version)
                .ToListAsync();
            return Ok(versions);
        }

        [HttpGet("requirement/version/{versionId}")]
        public async Task<ActionResult> GetRequirementVersion(int versionId)
        {
            var version = await _db.RequirementVersions.FindAsync(versionId);
            if (version == null) return NotFound();
            return Ok(version);
        }

        [HttpGet("requirement/redline/{oldVersionId}/{newVersionId}")]
        public async Task<ActionResult<RedlineResultDto>> RedlineRequirement(int oldVersionId, int newVersionId)
        {
            var oldV = await _db.RequirementVersions.FindAsync(oldVersionId);
            var newV = await _db.RequirementVersions.FindAsync(newVersionId);
            if (oldV == null || newV == null) return NotFound();
            var diff = _redlineService.CompareRequirements(oldV, newV);
            return Ok(diff);
        }

        // --- Test Case Version Endpoints ---

        [HttpGet("testcase/{testCaseId}/versions")]
        public async Task<ActionResult> GetTestCaseVersions(int testCaseId)
        {
            var versions = await _db.TestCaseVersions
                .Where(v => v.TestCaseId == testCaseId)
                .OrderBy(v => v.Version)
                .ToListAsync();
            return Ok(versions);
        }

        [HttpGet("testcase/version/{versionId}")]
        public async Task<ActionResult> GetTestCaseVersion(int versionId)
        {
            var version = await _db.TestCaseVersions.FindAsync(versionId);
            if (version == null) return NotFound();
            return Ok(version);
        }

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
