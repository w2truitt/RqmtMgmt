using RqmtMgmtShared;
using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    /// <summary>
    /// API controller for managing test cases.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class TestCaseController : ControllerBase
    {
        private readonly RqmtMgmtShared.ITestCaseService _testCaseService;

        public TestCaseController(RqmtMgmtShared.ITestCaseService testCaseService)
        {
            _testCaseService = testCaseService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TestCaseDto>>> GetAll()
        {
            var cases = await _testCaseService.GetAllAsync();
            return Ok(cases);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TestCaseDto>> GetById(int id)
        {
            var testCase = await _testCaseService.GetByIdAsync(id);
            if (testCase == null) return NotFound();
            return Ok(testCase);
        }

        [HttpPost]
        public async Task<ActionResult<TestCaseDto>> Create([FromBody] TestCaseDto dto)
        {
            var created = await _testCaseService.CreateAsync(dto);
            if (created == null) return BadRequest();
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] TestCaseDto dto)
        {
            if (id != dto.Id) return BadRequest();
            var success = await _testCaseService.UpdateAsync(dto);
            if (!success) return NotFound();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _testCaseService.DeleteAsync(id);
            if (!deleted) return NotFound();
            return NoContent();
        }
    }
}
