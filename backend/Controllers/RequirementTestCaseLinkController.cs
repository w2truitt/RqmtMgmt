using System.Collections.Generic;
using System.Threading.Tasks;
using backend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using RqmtMgmtShared;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class RequirementTestCaseLinkController : ControllerBase
    {
        private readonly RqmtMgmtShared.IRequirementTestCaseLinkService _service;
        public RequirementTestCaseLinkController(RqmtMgmtShared.IRequirementTestCaseLinkService service) => _service = service;

        [HttpGet("requirement/{requirementId}")]
        public async Task<ActionResult<List<RequirementTestCaseLinkDto>>> GetLinksForRequirement(int requirementId)
        {
            var links = await _service.GetLinksForRequirement(requirementId);
            return Ok(links);
        }

        [HttpGet("testcase/{testCaseId}")]
        public async Task<ActionResult<List<RequirementTestCaseLinkDto>>> GetLinksForTestCase(int testCaseId)
        {
            var links = await _service.GetLinksForTestCase(testCaseId);
            return Ok(links);
        }

        [HttpPost]
        public async Task<IActionResult> AddLink([FromBody] RequirementTestCaseLinkDto dto)
        {
            await _service.AddLink(dto.RequirementId, dto.TestCaseId);
            return NoContent();
        }

        [HttpPost("{requirementId}/{testCaseId}")]
        public async Task<IActionResult> AddLink(int requirementId, int testCaseId)
        {
            await _service.AddLink(requirementId, testCaseId);
            return NoContent();
        }

        [HttpDelete]
        public async Task<IActionResult> RemoveLink([FromQuery] int requirementId, [FromQuery] int testCaseId)
        {
            await _service.RemoveLink(requirementId, testCaseId);
            return NoContent();
        }
    }
}