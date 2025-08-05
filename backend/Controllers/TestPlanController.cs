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
        private readonly RqmtMgmtShared.ITestPlanService _testPlanService;
        public TestPlanController(RqmtMgmtShared.ITestPlanService testPlanService)
        {
            _testPlanService = testPlanService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TestPlanDto>>> GetAll()
        {
            var plans = await _testPlanService.GetAllAsync();
            return Ok(plans);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TestPlanDto>> GetById(int id)
        {
            var plan = await _testPlanService.GetByIdAsync(id);
            if (plan == null) return NotFound();
            return Ok(plan);
        }

        [HttpPost]
        public async Task<ActionResult<TestPlanDto>> Create([FromBody] TestPlanDto dto)
        {
            var created = await _testPlanService.CreateAsync(dto);
            if (created == null) return BadRequest();
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] TestPlanDto dto)
        {
            if (id != dto.Id) return BadRequest();
            var success = await _testPlanService.UpdateAsync(dto);
            if (!success) return NotFound();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _testPlanService.DeleteAsync(id);
            if (!deleted) return NotFound();
            return NoContent();
        }
    }
}
