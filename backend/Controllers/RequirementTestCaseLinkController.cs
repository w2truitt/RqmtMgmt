using System.Collections.Generic;
using System.Threading.Tasks;
using backend.Services;
using Microsoft.AspNetCore.Mvc;
using RqmtMgmtShared;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RequirementTestCaseLinkController : ControllerBase
    {
        private readonly IRequirementTestCaseLinkService _service;
        public RequirementTestCaseLinkController(IRequirementTestCaseLinkService service) => _service = service;

        [HttpGet("requirement/{requirementId}")]
        public async Task<IActionResult> GetLinksForRequirement(int requirementId)
            => Ok(await _service.GetLinksForRequirementAsync(requirementId));

        [HttpGet("testcase/{testCaseId}")]
        public async Task<IActionResult> GetLinksForTestCase(int testCaseId)
            => Ok(await _service.GetLinksForTestCaseAsync(testCaseId));

        [HttpPost]
        public async Task<IActionResult> CreateLink([FromBody] RequirementTestCaseLinkDto dto)
            => await _service.CreateLinkAsync(dto)
                ? Created($"api/RequirementTestCaseLink/requirement/{dto.RequirementId}", dto)
                : BadRequest();

        [HttpDelete]
        public async Task<IActionResult> DeleteLink([FromQuery] int requirementId, [FromQuery] int testCaseId)
            => await _service.DeleteLinkAsync(requirementId, testCaseId)
                ? NoContent()
                : NotFound();
    }
}
