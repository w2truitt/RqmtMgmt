using backend.Services;
using Microsoft.AspNetCore.Mvc;
using RqmtMgmtShared;

namespace backend.Controllers
{
    /// <summary>
    /// API controller for managing document sections with full CRUD operations.
    /// Provides endpoints for creating, reading, updating, and deleting sections within documents.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class DocumentSectionsController : ControllerBase
    {
        private readonly IDocumentSectionService _documentSectionService;

        /// <summary>
        /// Initializes a new instance of the DocumentSectionsController with the specified service.
        /// </summary>
        /// <param name="documentSectionService">The service for document section operations.</param>
        public DocumentSectionsController(IDocumentSectionService documentSectionService)
        {
            _documentSectionService = documentSectionService;
        }

        /// <summary>
        /// Retrieves all sections for a specific document.
        /// </summary>
        /// <param name="documentId">The ID of the document.</param>
        /// <returns>A list of sections for the specified document, ordered by section order.</returns>
        /// <response code="200">Returns the list of sections for the document.</response>
        [HttpGet("document/{documentId}")]
        public async Task<ActionResult<IEnumerable<DocumentSectionDto>>> GetByDocumentId(int documentId)
        {
            var sections = await _documentSectionService.GetByDocumentIdAsync(documentId);
            return Ok(sections);
        }

        /// <summary>
        /// Retrieves a specific document section by its ID.
        /// </summary>
        /// <param name="id">The ID of the document section.</param>
        /// <returns>The document section if found.</returns>
        /// <response code="200">Returns the document section.</response>
        /// <response code="404">If the document section is not found.</response>
        [HttpGet("{id}")]
        public async Task<ActionResult<DocumentSectionDto>> GetById(int id)
        {
            var section = await _documentSectionService.GetByIdAsync(id);
            if (section == null)
                return NotFound();

            return Ok(section);
        }

        /// <summary>
        /// Creates a new document section.
        /// </summary>
        /// <param name="section">The document section to create.</param>
        /// <returns>The created document section with assigned ID.</returns>
        /// <response code="201">Returns the newly created document section.</response>
        /// <response code="400">If the document section data is invalid.</response>
        [HttpPost]
        public async Task<ActionResult<DocumentSectionDto>> Create([FromBody] DocumentSectionDto section)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var createdSection = await _documentSectionService.CreateAsync(section);
            if (createdSection == null)
                return BadRequest("Failed to create document section");

            return CreatedAtAction(nameof(GetById), new { id = createdSection.Id }, createdSection);
        }

        /// <summary>
        /// Updates an existing document section.
        /// </summary>
        /// <param name="id">The ID of the document section to update.</param>
        /// <param name="section">The updated document section data.</param>
        /// <returns>No content if successful.</returns>
        /// <response code="204">If the update was successful.</response>
        /// <response code="400">If the document section data is invalid or IDs don't match.</response>
        /// <response code="404">If the document section is not found.</response>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] DocumentSectionDto section)
        {
            if (id != section.Id)
                return BadRequest("ID mismatch");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var success = await _documentSectionService.UpdateAsync(section);
            if (!success)
                return NotFound();

            return NoContent();
        }

        /// <summary>
        /// Deletes a document section by its ID.
        /// </summary>
        /// <param name="id">The ID of the document section to delete.</param>
        /// <returns>No content if successful.</returns>
        /// <response code="204">If the deletion was successful.</response>
        /// <response code="404">If the document section is not found.</response>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _documentSectionService.DeleteAsync(id);
            if (!success)
                return NotFound();

            return NoContent();
        }

        /// <summary>
        /// Reorders sections within a document based on the provided section IDs.
        /// </summary>
        /// <param name="documentId">The ID of the document.</param>
        /// <param name="sectionIds">The list of section IDs in the desired order.</param>
        /// <returns>No content if successful.</returns>
        /// <response code="204">If the reordering was successful.</response>
        /// <response code="400">If the section IDs are invalid or incomplete.</response>
        [HttpPost("document/{documentId}/reorder")]
        public async Task<IActionResult> ReorderSections(int documentId, [FromBody] List<int> sectionIds)
        {
            if (sectionIds == null || sectionIds.Count == 0)
                return BadRequest("Section IDs are required");

            var success = await _documentSectionService.ReorderSectionsAsync(documentId, sectionIds);
            if (!success)
                return BadRequest("Failed to reorder sections");

            return NoContent();
        }
    }
}