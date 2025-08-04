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
        private readonly IRequirementService _requirementService;

        /// <summary>
        /// Initializes a new instance of the <see cref="RequirementController"/> class.
        /// </summary>
        public RequirementController(IRequirementService requirementService)
        {
            _requirementService = requirementService;
        }

        /// <summary>
        /// Gets all requirements.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RequirementDto>>> GetAll()
        {
            var requirements = await _requirementService.GetAllAsync();
            var dtos = requirements.Select(r => ToDto(r));
            return Ok(dtos);
        }

        /// <summary>
        /// Gets a requirement by its unique identifier.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<RequirementDto>> GetById(int id)
        {
            var requirement = await _requirementService.GetByIdAsync(id);
            if (requirement == null) return NotFound();
            return Ok(ToDto(requirement));
        }

        /// <summary>
        /// Creates a new requirement.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<RequirementDto>> Create([FromBody] RequirementDto dto)
        {
            var model = FromDto(dto);
            var created = await _requirementService.CreateAsync(model);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, ToDto(created));
        }

        /// <summary>
        /// Updates an existing requirement.
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<RequirementDto>> Update(int id, [FromBody] RequirementDto dto)
        {
            if (id != dto.Id) return BadRequest();
            var existing = await _requirementService.GetByIdAsync(id);
            if (existing == null) return NotFound();
            var updatedModel = FromDto(dto);
            var updated = await _requirementService.UpdateAsync(updatedModel);
            return Ok(ToDto(updated));
        }

        /// <summary>
        /// Deletes a requirement by its unique identifier.
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _requirementService.DeleteAsync(id);
            if (!deleted) return NotFound();
            return NoContent();
        }

        // Mapping helpers
        private static RequirementDto ToDto(Requirement r) => new RequirementDto
        {
            Id = r.Id,
            Type = r.Type.ToString(),
            Title = r.Title,
            Description = r.Description,
            ParentId = r.ParentId,
            Status = r.Status.ToString(),
            Version = r.Version,
            CreatedBy = r.CreatedBy,
            CreatedAt = r.CreatedAt,
            UpdatedAt = r.UpdatedAt
        };
        private static Requirement FromDto(RequirementDto dto) => new Requirement
        {
            Id = dto.Id,
            Type = Enum.Parse<RequirementType>(dto.Type),
            Title = dto.Title,
            Description = dto.Description,
            ParentId = dto.ParentId,
            Status = Enum.Parse<RequirementStatus>(dto.Status),
            Version = dto.Version,
            CreatedBy = dto.CreatedBy,
            CreatedAt = dto.CreatedAt,
            UpdatedAt = dto.UpdatedAt
        };
    }
}
