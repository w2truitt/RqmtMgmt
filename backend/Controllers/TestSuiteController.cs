using RqmtMgmtShared;
using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestSuiteController : ControllerBase
    {
        private readonly RqmtMgmtShared.ITestSuiteService _testSuiteService;
        public TestSuiteController(RqmtMgmtShared.ITestSuiteService testSuiteService)
        {
            _testSuiteService = testSuiteService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TestSuiteDto>>> GetAll()
        {
            var suites = await _testSuiteService.GetAllAsync();
            return Ok(suites);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TestSuiteDto>> GetById(int id)
        {
            var suite = await _testSuiteService.GetByIdAsync(id);
            if (suite == null) return NotFound();
            return Ok(suite);
        }

        [HttpPost]
        public async Task<ActionResult<TestSuiteDto>> Create([FromBody] TestSuiteDto dto)
        {
            var created = await _testSuiteService.CreateAsync(dto);
            if (created == null) return BadRequest();
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] TestSuiteDto dto)
        {
            if (id != dto.Id) return BadRequest();
            var success = await _testSuiteService.UpdateAsync(dto);
            if (!success) return NotFound();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _testSuiteService.DeleteAsync(id);
            if (!deleted) return NotFound();
            return NoContent();
        }
    }
}
