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
        private readonly ITestSuiteService _testSuiteService;
        public TestSuiteController(ITestSuiteService testSuiteService)
        {
            _testSuiteService = testSuiteService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TestSuiteDto>>> GetAll()
        {
            var suites = await _testSuiteService.GetAllAsync();
            var dtos = suites.Select(ToDto);
            return Ok(dtos);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TestSuiteDto>> GetById(int id)
        {
            var suite = await _testSuiteService.GetByIdAsync(id);
            if (suite == null) return NotFound();
            return Ok(ToDto(suite));
        }

        [HttpPost]
        public async Task<ActionResult<TestSuiteDto>> Create([FromBody] TestSuiteDto dto)
        {
            var model = FromDto(dto);
            var created = await _testSuiteService.CreateAsync(model);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, ToDto(created));
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<TestSuiteDto>> Update(int id, [FromBody] TestSuiteDto dto)
        {
            if (id != dto.Id) return BadRequest();
            var existing = await _testSuiteService.GetByIdAsync(id);
            if (existing == null) return NotFound();
            var updated = await _testSuiteService.UpdateAsync(FromDto(dto));
            return Ok(ToDto(updated));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _testSuiteService.DeleteAsync(id);
            if (!deleted) return NotFound();
            return NoContent();
        }

        private static TestSuiteDto ToDto(TestSuite s) => new TestSuiteDto
        {
            Id = s.Id,
            Name = s.Name,
            Description = s.Description,
            CreatedBy = s.CreatedBy,
            CreatedAt = s.CreatedAt
        };
        private static TestSuite FromDto(TestSuiteDto dto) => new TestSuite
        {
            Id = dto.Id,
            Name = dto.Name,
            Description = dto.Description,
            CreatedBy = dto.CreatedBy,
            CreatedAt = dto.CreatedAt
        };
    }
}
