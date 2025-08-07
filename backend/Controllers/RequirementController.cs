using RqmtMgmtShared;
using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    /// <summary>
    /// API controller for managing requirements with full CRUD operations.
    /// Provides endpoints for creating, reading, updating, and deleting requirements.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class RequirementController : ControllerBase
    {
        private readonly RqmtMgmtShared.IRequirementService _requirementService;

        /// <summary>
        /// Initializes a new instance of the RequirementController with the specified requirement service.
        /// </summary>
        /// <param name="requirementService">The service for requirement operations.</param>
        public RequirementController(RqmtMgmtShared.IRequirementService requirementService)
        {
            _requirementService = requirementService;
        }

        /// <summary>
        /// Retrieves all requirements from the system.
        /// </summary>
        /// <returns>A list of all requirements.</returns>
        /// <response code="200">Returns the list of requirements.</response>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RequirementDto>>> GetAll()
        {
            var requirements = await _requirementService.GetAllAsync();
            return Ok(requirements);
        }

        /// <summary>
        /// Retrieves a paginated list of requirements with optional filtering and sorting.
        /// </summary>
        /// <param name="pageNumber">The page number to retrieve (1-based).</param>
        /// <param name="pageSize">The number of items per page (1-100).</param>
        /// <param name="searchTerm">Optional search term to filter by title or description.</param>
        /// <param name="sortBy">Optional field to sort by (title, status, type, createdat, updatedat).</param>
        /// <param name="sortDescending">Whether to sort in descending order.</param>
        /// <returns>A paginated result containing requirements and pagination metadata.</returns>
        /// <response code="200">Returns the paginated list of requirements.</response>
        [HttpGet("paged")]
        public async Task<ActionResult<PagedResult<RequirementDto>>> GetPaged(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? searchTerm = null,
            [FromQuery] string? sortBy = null,
            [FromQuery] bool sortDescending = false)
        {
            var parameters = new PaginationParameters
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                SearchTerm = searchTerm,
                SortBy = sortBy,
                SortDescending = sortDescending
            };

            var pagedResult = await _requirementService.GetPagedAsync(parameters);
            return Ok(pagedResult);
        }

        /// <summary>
        /// Retrieves a specific requirement by its ID.
        /// </summary>
        /// <param name="id">The unique identifier of the requirement.</param>
        /// <returns>The requirement if found.</returns>
        /// <response code="200">Returns the requested requirement.</response>
        /// <response code="404">If the requirement is not found.</response>
        [HttpGet("{id}")]
        public async Task<ActionResult<RequirementDto>> GetById(int id)
        {
            var requirement = await _requirementService.GetByIdAsync(id);
            if (requirement == null) return NotFound();
            return Ok(requirement);
        }

        /// <summary>
        /// Creates a new requirement in the system.
        /// </summary>
        /// <param name="dto">The requirement data to create.</param>
        /// <returns>The created requirement with its assigned ID.</returns>
        /// <response code="201">Returns the newly created requirement.</response>
        /// <response code="400">If the requirement data is invalid or creation fails.</response>
        [HttpPost]
        public async Task<ActionResult<RequirementDto>> Create([FromBody] RequirementDto dto)
        {
            var created = await _requirementService.CreateAsync(dto);
            if (created == null) return BadRequest();
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        /// <summary>
        /// Updates an existing requirement with new data.
        /// </summary>
        /// <param name="id">The ID of the requirement to update.</param>
        /// <param name="dto">The updated requirement data.</param>
        /// <returns>No content if successful.</returns>
        /// <response code="204">If the requirement was successfully updated.</response>
        /// <response code="400">If the ID in the URL doesn't match the ID in the request body.</response>
        /// <response code="404">If the requirement is not found.</response>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] RequirementDto dto)
        {
            if (id != dto.Id) return BadRequest();
            var success = await _requirementService.UpdateAsync(dto);
            if (!success) return NotFound();
            return NoContent();
        }

        /// <summary>
        /// Deletes a requirement from the system.
        /// </summary>
        /// <param name="id">The ID of the requirement to delete.</param>
        /// <returns>No content if successful.</returns>
        /// <response code="204">If the requirement was successfully deleted.</response>
        /// <response code="404">If the requirement is not found.</response>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _requirementService.DeleteAsync(id);
            if (!deleted) return NotFound();
            return NoContent();
        }
    }
}