using backend.Services;
using Microsoft.AspNetCore.Mvc;
using RqmtMgmtShared;

namespace backend.Controllers
{
    /// <summary>
    /// API controller for managing requirement documents with full CRUD operations.
    /// Provides endpoints for creating, reading, updating, and deleting documents (CRD, PRD, SRS).
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class DocumentsController : ControllerBase
    {
        private readonly IDocumentService _documentService;

        /// <summary>
        /// Initializes a new instance of the DocumentsController with the specified document service.
        /// </summary>
        /// <param name="documentService">The service for document operations.</param>
        public DocumentsController(IDocumentService documentService)
        {
            _documentService = documentService;
        }

        /// <summary>
        /// Retrieves all documents from the system.
        /// </summary>
        /// <returns>A list of all documents.</returns>
        /// <response code="200">Returns the list of documents.</response>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DocumentDto>>> GetAll()
        {
            var documents = await _documentService.GetAllAsync();
            return Ok(documents);
        }

        /// <summary>
        /// Retrieves a paginated list of documents with optional filtering and sorting.
        /// </summary>
        /// <param name="pageNumber">The page number to retrieve (1-based).</param>
        /// <param name="pageSize">The number of items per page (1-100).</param>
        /// <param name="searchTerm">Optional search term to filter by title, owner, or objective.</param>
        /// <param name="sortBy">Optional field to sort by (title, type, status, createdat, updatedat).</param>
        /// <param name="sortDescending">Whether to sort in descending order.</param>
        /// <returns>A paginated result containing documents and pagination metadata.</returns>
        /// <response code="200">Returns the paginated list of documents.</response>
        [HttpGet("paged")]
        public async Task<ActionResult<PagedResult<DocumentDto>>> GetPaged(
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

            var result = await _documentService.GetPagedAsync(parameters);
            return Ok(result);
        }

        /// <summary>
        /// Retrieves all documents for a specific project.
        /// </summary>
        /// <param name="projectId">The ID of the project.</param>
        /// <returns>A list of documents for the specified project.</returns>
        /// <response code="200">Returns the list of documents for the project.</response>
        [HttpGet("project/{projectId}")]
        public async Task<ActionResult<IEnumerable<DocumentDto>>> GetByProjectId(int projectId)
        {
            var documents = await _documentService.GetByProjectIdAsync(projectId);
            return Ok(documents);
        }

        /// <summary>
        /// Retrieves documents for a specific project with pagination.
        /// </summary>
        /// <param name="projectId">The ID of the project.</param>
        /// <param name="pageNumber">The page number to retrieve (1-based).</param>
        /// <param name="pageSize">The number of items per page (1-100).</param>
        /// <param name="searchTerm">Optional search term to filter documents.</param>
        /// <param name="sortBy">Optional field to sort by.</param>
        /// <param name="sortDescending">Whether to sort in descending order.</param>
        /// <returns>A paginated result of documents for the specified project.</returns>
        /// <response code="200">Returns the paginated list of documents for the project.</response>
        [HttpGet("project/{projectId}/paged")]
        public async Task<ActionResult<PagedResult<DocumentDto>>> GetPagedByProjectId(
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

            var result = await _documentService.GetPagedByProjectIdAsync(projectId, parameters);
            return Ok(result);
        }

        /// <summary>
        /// Retrieves all documents of a specific type.
        /// </summary>
        /// <param name="type">The document type (CRD, PRD, SRS).</param>
        /// <returns>A list of documents of the specified type.</returns>
        /// <response code="200">Returns the list of documents of the specified type.</response>
        [HttpGet("type/{type}")]
        public async Task<ActionResult<IEnumerable<DocumentDto>>> GetByType(DocumentType type)
        {
            var documents = await _documentService.GetByTypeAsync(type);
            return Ok(documents);
        }

        /// <summary>
        /// Retrieves a specific document by its ID.
        /// </summary>
        /// <param name="id">The ID of the document.</param>
        /// <returns>The document if found.</returns>
        /// <response code="200">Returns the document.</response>
        /// <response code="404">If the document is not found.</response>
        [HttpGet("{id}")]
        public async Task<ActionResult<DocumentDto>> GetById(int id)
        {
            var document = await _documentService.GetByIdAsync(id);
            if (document == null)
                return NotFound();

            return Ok(document);
        }

        /// <summary>
        /// Creates a new document.
        /// </summary>
        /// <param name="document">The document to create.</param>
        /// <returns>The created document with assigned ID.</returns>
        /// <response code="201">Returns the newly created document.</response>
        /// <response code="400">If the document data is invalid.</response>
        [HttpPost]
        public async Task<ActionResult<DocumentDto>> Create([FromBody] DocumentDto document)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var createdDocument = await _documentService.CreateAsync(document);
            if (createdDocument == null)
                return BadRequest("Failed to create document");

            return CreatedAtAction(nameof(GetById), new { id = createdDocument.Id }, createdDocument);
        }

        /// <summary>
        /// Updates an existing document.
        /// </summary>
        /// <param name="id">The ID of the document to update.</param>
        /// <param name="document">The updated document data.</param>
        /// <returns>No content if successful.</returns>
        /// <response code="204">If the update was successful.</response>
        /// <response code="400">If the document data is invalid or IDs don't match.</response>
        /// <response code="404">If the document is not found.</response>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] DocumentDto document)
        {
            if (id != document.Id)
                return BadRequest("ID mismatch");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var success = await _documentService.UpdateAsync(document);
            if (!success)
                return NotFound();

            return NoContent();
        }

        /// <summary>
        /// Deletes a document by its ID.
        /// </summary>
        /// <param name="id">The ID of the document to delete.</param>
        /// <returns>No content if successful.</returns>
        /// <response code="204">If the deletion was successful.</response>
        /// <response code="404">If the document is not found.</response>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _documentService.DeleteAsync(id);
            if (!success)
                return NotFound();

            return NoContent();
        }

        /// <summary>
        /// Retrieves traceability matrix data for a document showing requirement trace relationships.
        /// </summary>
        /// <param name="id">The ID of the document.</param>
        /// <param name="direction">The trace direction: 'upstream' (requirements that trace TO this document) or 'downstream' (requirements that trace FROM this document).</param>
        /// <param name="uncoveredOnly">If true, returns only requirements without trace links (for audit purposes).</param>
        /// <returns>Traceability matrix data for the document.</returns>
        /// <response code="200">Returns the traceability matrix data.</response>
        /// <response code="404">If the document is not found.</response>
        /// <response code="400">If the direction parameter is invalid.</response>
        [HttpGet("{id}/traceability")]
        public async Task<ActionResult<TraceabilityMatrixDto>> GetTraceability(
            int id,
            [FromQuery] string direction,
            [FromQuery] bool uncoveredOnly = false)
        {
            // Validate direction parameter
            if (string.IsNullOrEmpty(direction) || 
                (!direction.Equals("upstream", StringComparison.OrdinalIgnoreCase) && 
                 !direction.Equals("downstream", StringComparison.OrdinalIgnoreCase)))
            {
                return BadRequest("Direction parameter must be 'upstream' or 'downstream'.");
            }

            var traceabilityMatrix = await _documentService.GetTraceabilityMatrixAsync(id, direction.ToLower(), uncoveredOnly);
            if (traceabilityMatrix == null)
                return NotFound();

            return Ok(traceabilityMatrix);
        }
    }
}