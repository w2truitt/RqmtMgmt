using RqmtMgmtShared;
using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    /// <summary>
    /// API controller for managing requirements.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class RequirementController : ControllerBase
    {
        private readonly RqmtMgmtShared.IRequirementService _requirementService;

        public RequirementController(RqmtMgmtShared.IRequirementService requirementService)
        {
            _requirementService = requirementService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<RequirementDto>>> GetAll()
        {
            var requirements = await _requirementService.GetAllAsync();
            return Ok(requirements);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<RequirementDto>> GetById(int id)
        {
            var requirement = await _requirementService.GetByIdAsync(id);
            if (requirement == null) return NotFound();
            return Ok(requirement);
        }

        [HttpPost]
        public async Task<ActionResult<RequirementDto>> Create([FromBody] RequirementDto dto)
        {
            var created = await _requirementService.CreateAsync(dto);
            if (created == null) return BadRequest();
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] RequirementDto dto)
        {
            if (id != dto.Id) return BadRequest();
            var success = await _requirementService.UpdateAsync(dto);
            if (!success) return NotFound();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _requirementService.DeleteAsync(id);
            if (!deleted) return NotFound();
            return NoContent();
        }
    }
}
