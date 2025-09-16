using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Mvc;
using RqmtMgmtShared;

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
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] string? sortBy = null,
            [FromQuery] bool sortDescending = false)
        {
            var parameters = new PaginationParameters
            {
                PageNumber = Math.Max(1, pageNumber),
                PageSize = Math.Min(100, Math.Max(1, pageSize)),
                SearchTerm = searchTerm,
                SortBy = sortBy,
                SortDescending = sortDescending
            };

            var result = await _requirementService.GetPagedAsync(parameters);
            return Ok(result);
        }

        /// <summary>
        /// Retrieves all requirements for a specific project.
        /// </summary>
        /// <param name="projectId">The ID of the project.</param>
        /// <returns>A list of requirements for the specified project.</returns>
        /// <response code="200">Returns the list of requirements for the project.</response>
        [HttpGet("project/{projectId}")]
        public async Task<ActionResult<IEnumerable<RequirementDto>>> GetByProjectId(int projectId)
        {
            var requirements = await _requirementService.GetByProjectIdAsync(projectId);
            return Ok(requirements);
        }

        /// <summary>
        /// Retrieves requirements for a specific project with pagination.
        /// </summary>
        /// <param name="projectId">The ID of the project.</param>
        /// <param name="pageNumber">The page number to retrieve (1-based).</param>
        /// <param name="pageSize">The number of items per page (1-100).</param>
        /// <param name="searchTerm">Optional search term to filter requirements.</param>
        /// <param name="sortBy">Optional field to sort by.</param>
        /// <param name="sortDescending">Whether to sort in descending order.</param>
        /// <returns>A paginated result of requirements for the specified project.</returns>
        /// <response code="200">Returns the paginated list of requirements for the project.</response>
        [HttpGet("project/{projectId}/paged")]
        public async Task<ActionResult<PagedResult<RequirementDto>>> GetPagedByProjectId(
            int projectId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] string? sortBy = null,
            [FromQuery] bool sortDescending = false)
        {
            var parameters = new PaginationParameters
            {
                PageNumber = Math.Max(1, pageNumber),
                PageSize = Math.Min(100, Math.Max(1, pageSize)),
                SearchTerm = searchTerm,
                SortBy = sortBy,
                SortDescending = sortDescending
            };

            var result = await _requirementService.GetPagedByProjectIdAsync(projectId, parameters);
            return Ok(result);
        }

        /// <summary>
        /// Retrieves a specific requirement by its ID.
        /// </summary>
        /// <param name="id">The ID of the requirement.</param>
        /// <returns>The requirement if found.</returns>
        /// <response code="200">Returns the requirement.</response>
        /// <response code="404">If the requirement is not found.</response>
        [HttpGet("{id}")]
        public async Task<ActionResult<RequirementDto>> GetById(int id)
        {
            var requirement = await _requirementService.GetByIdAsync(id);
            if (requirement == null) return NotFound();
            return Ok(requirement);
        }

        /// <summary>
        /// Creates a new requirement.
        /// </summary>
        /// <param name="dto">The requirement to create.</param>
        /// <returns>The created requirement with assigned ID.</returns>
        /// <response code="201">Returns the newly created requirement.</response>
        /// <response code="400">If the requirement data is invalid.</response>
        [HttpPost]
        public async Task<ActionResult<RequirementDto>> Create([FromBody] RequirementDto dto)
        {
            var created = await _requirementService.CreateAsync(dto);
            if (created == null) return BadRequest();
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        /// <summary>
        /// Updates an existing requirement.
        /// </summary>
        /// <param name="id">The ID of the requirement to update.</param>
        /// <param name="dto">The updated requirement data.</param>
        /// <returns>No content if successful.</returns>
        /// <response code="204">If the update was successful.</response>
        /// <response code="400">If the requirement data is invalid or IDs don't match.</response>
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

        /// <summary>
        /// Retrieves all requirements for a specific document.
        /// </summary>
        /// <param name="documentId">The ID of the document.</param>
        /// <returns>A list of requirements for the specified document.</returns>
        /// <response code="200">Returns the list of requirements for the document.</response>
        [HttpGet("document/{documentId}")]
        public async Task<ActionResult<IEnumerable<RequirementDto>>> GetByDocumentId(int documentId)
        {
            var requirements = await _requirementService.GetByDocumentIdAsync(documentId);
            return Ok(requirements);
        }

        /// <summary>
        /// Retrieves all requirements for a specific document section.
        /// </summary>
        /// <param name="sectionId">The ID of the document section.</param>
        /// <returns>A list of requirements for the specified section.</returns>
        /// <response code="200">Returns the list of requirements for the section.</response>
        [HttpGet("section/{sectionId}")]
        public async Task<ActionResult<IEnumerable<RequirementDto>>> GetBySectionId(int sectionId)
        {
            var requirements = await _requirementService.GetBySectionIdAsync(sectionId);
            return Ok(requirements);
        }

        /// <summary>
        /// Retrieves requirements for a specific document with pagination.
        /// </summary>
        /// <param name="documentId">The ID of the document.</param>
        /// <param name="pageNumber">The page number to retrieve (1-based).</param>
        /// <param name="pageSize">The number of items per page (1-100).</param>
        /// <param name="searchTerm">Optional search term to filter requirements.</param>
        /// <param name="sortBy">Optional field to sort by.</param>
        /// <param name="sortDescending">Whether to sort in descending order.</param>
        /// <returns>A paginated result of requirements for the specified document.</returns>
        /// <response code="200">Returns the paginated list of requirements for the document.</response>
        [HttpGet("document/{documentId}/paged")]
        public async Task<ActionResult<PagedResult<RequirementDto>>> GetPagedByDocumentId(
            int documentId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] string? sortBy = null,
            [FromQuery] bool sortDescending = false)
        {
            var parameters = new PaginationParameters
            {
                PageNumber = Math.Max(1, pageNumber),
                PageSize = Math.Min(100, Math.Max(1, pageSize)),
                SearchTerm = searchTerm,
                SortBy = sortBy,
                SortDescending = sortDescending
            };

            var result = await _requirementService.GetPagedByDocumentIdAsync(documentId, parameters);
            return Ok(result);
        }

        /// <summary>
        /// Retrieves requirements for a specific document section with pagination.
        /// </summary>
        /// <param name="sectionId">The ID of the document section.</param>
        /// <param name="pageNumber">The page number to retrieve (1-based).</param>
        /// <param name="pageSize">The number of items per page (1-100).</param>
        /// <param name="searchTerm">Optional search term to filter requirements.</param>
        /// <param name="sortBy">Optional field to sort by.</param>
        /// <param name="sortDescending">Whether to sort in descending order.</param>
        /// <returns>A paginated result of requirements for the specified section.</returns>
        /// <response code="200">Returns the paginated list of requirements for the section.</response>
        [HttpGet("section/{sectionId}/paged")]
        public async Task<ActionResult<PagedResult<RequirementDto>>> GetPagedBySectionId(
            int sectionId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] string? sortBy = null,
            [FromQuery] bool sortDescending = false)
        {
            var parameters = new PaginationParameters
            {
                PageNumber = Math.Max(1, pageNumber),
                PageSize = Math.Min(100, Math.Max(1, pageSize)),
                SearchTerm = searchTerm,
                SortBy = sortBy,
                SortDescending = sortDescending
            };

            var result = await _requirementService.GetPagedBySectionIdAsync(sectionId, parameters);
            return Ok(result);
        }

        /// <summary>
        /// Retrieves version history for a specific requirement.
        /// </summary>
        /// <param name="requirementId">The ID of the requirement.</param>
        /// <returns>A list of requirement versions ordered by version number.</returns>
        /// <response code="200">Returns the list of requirement versions.</response>
        /// <response code="404">If the requirement is not found.</response>
        [HttpGet("{requirementId}/versions")]
        public async Task<ActionResult<IEnumerable<RequirementVersionDto>>> GetVersions(int requirementId)
        {
            var versions = await _requirementService.GetVersionsAsync(requirementId);
            return Ok(versions);
        }
    }
}