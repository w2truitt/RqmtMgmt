using RqmtMgmtShared;
using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestPlanController : ControllerBase
    {
        private readonly ITestPlanService _testPlanService;
        public TestPlanController(ITestPlanService testPlanService)
        {
            _testPlanService = testPlanService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TestPlanDto>>> GetAll()
        {
            var plans = await _testPlanService.GetAllAsync();
            var dtos = plans.Select(ToDto);
            return Ok(dtos);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TestPlanDto>> GetById(int id)
        {
            var plan = await _testPlanService.GetByIdAsync(id);
            if (plan == null) return NotFound();
            return Ok(ToDto(plan));
        }

        [HttpPost]
        public async Task<ActionResult<TestPlanDto>> Create([FromBody] TestPlanDto dto)
        {
            var model = FromDto(dto);
            var created = await _testPlanService.CreateAsync(model);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, ToDto(created));
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<TestPlanDto>> Update(int id, [FromBody] TestPlanDto dto)
        {
            if (id != dto.Id) return BadRequest();
            var existing = await _testPlanService.GetByIdAsync(id);
            if (existing == null) return NotFound();
            var updated = await _testPlanService.UpdateAsync(FromDto(dto));
            return Ok(ToDto(updated));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _testPlanService.DeleteAsync(id);
            if (!deleted) return NotFound();
            return NoContent();
        }

        private static TestPlanDto ToDto(TestPlan tp) => new TestPlanDto
        {
            Id = tp.Id,
            Name = tp.Name,
            Type = tp.Type.ToString(),
            Description = tp.Description,
            CreatedBy = tp.CreatedBy,
            CreatedAt = tp.CreatedAt
        };
        private static TestPlan FromDto(TestPlanDto dto) => new TestPlan
        {
            Id = dto.Id,
            Name = dto.Name,
            Type = Enum.Parse<TestPlanType>(dto.Type),
            Description = dto.Description,
            CreatedBy = dto.CreatedBy,
            CreatedAt = dto.CreatedAt
        };
    }
}
