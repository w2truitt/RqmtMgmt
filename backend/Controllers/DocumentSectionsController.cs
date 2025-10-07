using backend.Services;
using Microsoft.AspNetCore.Mvc;
using RqmtMgmtShared;

namespace backend.Controllers
{
    /// <summary>
    /// API controller for managing document sections with full CRUD operations and hierarchical support.
    /// Provides endpoints for creating, reading, updating, and deleting sections within documents,
    /// as well as managing hierarchical section relationships.
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

        #region Basic CRUD Endpoints

        /// <summary>
        /// Retrieves sections for a specific document.
        /// </summary>
        /// <param name="documentId">The ID of the document.</param>
        /// <param name="includeChildren">If true, returns hierarchical tree structure; if false, returns flat list.</param>
        /// <returns>A list or tree of sections for the specified document.</returns>
        /// <response code="200">Returns the sections for the document.</response>
        [HttpGet("document/{documentId}")]
        public async Task<ActionResult<IEnumerable<DocumentSectionDto>>> GetByDocumentId(
            int documentId, 
            [FromQuery] bool includeChildren = false)
        {
            if (includeChildren)
            {
                var hierarchy = await _documentSectionService.GetSectionHierarchyAsync(documentId);
                return Ok(hierarchy);
            }
            else
            {
                var sections = await _documentSectionService.GetByDocumentIdAsync(documentId);
                return Ok(sections);
            }
        }

        /// <summary>
        /// Retrieves a specific document section by its ID.
        /// </summary>
        /// <param name="id">The ID of the document section.</param>
        /// <param name="includeChildren">If true, includes child sections; if false, returns only the section.</param>
        /// <param name="depth">The depth of children to include (-1 for all, 0 for none, 1+ for levels).</param>
        /// <returns>The document section if found.</returns>
        /// <response code="200">Returns the document section.</response>
        /// <response code="404">If the document section is not found.</response>
        [HttpGet("{id}")]
        public async Task<ActionResult<DocumentSectionDto>> GetById(
            int id,
            [FromQuery] bool includeChildren = false,
            [FromQuery] int depth = -1)
        {
            if (includeChildren)
            {
                var section = await _documentSectionService.GetSectionWithChildrenAsync(id, depth);
                if (section == null)
                    return NotFound();
                return Ok(section);
            }
            else
            {
                var section = await _documentSectionService.GetByIdAsync(id);
                if (section == null)
                    return NotFound();
                return Ok(section);
            }
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
                return BadRequest("Failed to create document section. Check parent reference and circular dependencies.");

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
        /// Will fail if section has child sections.
        /// </summary>
        /// <param name="id">The ID of the document section to delete.</param>
        /// <returns>No content if successful.</returns>
        /// <response code="204">If the deletion was successful.</response>
        /// <response code="400">If the section has children.</response>
        /// <response code="404">If the document section is not found.</response>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _documentSectionService.DeleteAsync(id);
            if (!success)
                return BadRequest("Cannot delete section. It may have child sections or not exist.");

            return NoContent();
        }

        #endregion

        #region Hierarchical Query Endpoints

        /// <summary>
        /// Retrieves the full hierarchical structure of sections for a document.
        /// </summary>
        /// <param name="documentId">The ID of the document.</param>
        /// <returns>A tree structure of sections.</returns>
        /// <response code="200">Returns the section hierarchy.</response>
        [HttpGet("document/{documentId}/hierarchy")]
        public async Task<ActionResult<IEnumerable<DocumentSectionDto>>> GetHierarchy(int documentId)
        {
            var hierarchy = await _documentSectionService.GetSectionHierarchyAsync(documentId);
            return Ok(hierarchy);
        }

        /// <summary>
        /// Retrieves direct child sections of a parent section.
        /// </summary>
        /// <param name="parentId">The ID of the parent section.</param>
        /// <returns>A list of child sections.</returns>
        /// <response code="200">Returns the child sections.</response>
        [HttpGet("{parentId}/children")]
        public async Task<ActionResult<IEnumerable<DocumentSectionDto>>> GetChildren(int parentId)
        {
            var children = await _documentSectionService.GetChildSectionsAsync(parentId);
            return Ok(children);
        }

        /// <summary>
        /// Retrieves root-level sections for a document.
        /// </summary>
        /// <param name="documentId">The ID of the document.</param>
        /// <returns>A list of root sections.</returns>
        /// <response code="200">Returns the root sections.</response>
        [HttpGet("document/{documentId}/roots")]
        public async Task<ActionResult<IEnumerable<DocumentSectionDto>>> GetRoots(int documentId)
        {
            var roots = await _documentSectionService.GetRootSectionsAsync(documentId);
            return Ok(roots);
        }

        #endregion

        #region Hierarchy Management Endpoints

        /// <summary>
        /// Moves a section to a new parent (or document root) and optionally reorders it.
        /// </summary>
        /// <param name="id">The ID of the section to move.</param>
        /// <param name="request">The move request containing new parent ID and order.</param>
        /// <returns>No content if successful.</returns>
        /// <response code="204">If the move was successful.</response>
        /// <response code="400">If the move would create a circular reference.</response>
        /// <response code="404">If the section is not found.</response>
        [HttpPost("{id}/move")]
        public async Task<IActionResult> MoveSection(int id, [FromBody] MoveSectionRequest request)
        {
            var success = await _documentSectionService.MoveSectionAsync(id, request.NewParentId, request.NewOrder);
            if (!success)
                return BadRequest("Failed to move section. Check for circular references.");

            return NoContent();
        }

        /// <summary>
        /// Reorders sections within a parent (or document root).
        /// </summary>
        /// <param name="request">The reorder request containing parent ID and section IDs.</param>
        /// <returns>No content if successful.</returns>
        /// <response code="204">If the reordering was successful.</response>
        /// <response code="400">If the section IDs are invalid or incomplete.</response>
        [HttpPost("reorder")]
        public async Task<IActionResult> ReorderSections([FromBody] ReorderSectionsRequest request)
        {
            if (request.SectionIds == null || request.SectionIds.Count == 0)
                return BadRequest("Section IDs are required");

            var success = await _documentSectionService.ReorderSectionsAsync(request.ParentId, request.SectionIds);
            if (!success)
                return BadRequest("Failed to reorder sections");

            return NoContent();
        }

        /// <summary>
        /// Validates if a parent reference is valid (no circular references).
        /// </summary>
        /// <param name="sectionId">The ID of the section.</param>
        /// <param name="parentId">The proposed parent ID.</param>
        /// <returns>True if valid, false otherwise.</returns>
        /// <response code="200">Returns validation result.</response>
        [HttpGet("{sectionId}/validate-parent/{parentId}")]
        public async Task<ActionResult<bool>> ValidateParent(int sectionId, int parentId)
        {
            var isValid = await _documentSectionService.ValidateParentReferenceAsync(sectionId, parentId);
            return Ok(isValid);
        }

        #endregion

        #region Utility Endpoints

        /// <summary>
        /// Generates a section number for a section based on its position in the hierarchy.
        /// </summary>
        /// <param name="id">The ID of the section.</param>
        /// <returns>The generated section number.</returns>
        /// <response code="200">Returns the section number.</response>
        /// <response code="404">If the section is not found.</response>
        [HttpGet("{id}/generate-number")]
        public async Task<ActionResult<string>> GenerateNumber(int id)
        {
            var number = await _documentSectionService.GenerateSectionNumberAsync(id);
            if (number == null)
                return NotFound();

            return Ok(number);
        }

        /// <summary>
        /// Gets the requirement count for a section.
        /// </summary>
        /// <param name="id">The ID of the section.</param>
        /// <param name="recursive">Whether to include requirements from child sections.</param>
        /// <returns>The requirement count.</returns>
        /// <response code="200">Returns the requirement count.</response>
        [HttpGet("{id}/requirement-count")]
        public async Task<ActionResult<int>> GetRequirementCount(int id, [FromQuery] bool recursive = false)
        {
            var count = await _documentSectionService.GetRequirementCountAsync(id, recursive);
            return Ok(count);
        }

        #endregion

        #region Enhanced Search Endpoints

        /// <summary>
        /// Searches for sections within a document using fuzzy or exact matching.
        /// </summary>
        /// <param name="documentId">The ID of the document to search within.</param>
        /// <param name="searchTerm">The search term to match against section titles.</param>
        /// <param name="fuzzyMatch">Whether to use fuzzy matching (default: true).</param>
        /// <param name="similarityThreshold">Minimum similarity score for fuzzy matches (0.0-1.0, default: 0.8).</param>
        /// <param name="parentId">Optional parent section ID to limit search scope.</param>
        /// <returns>A list of matching sections ordered by relevance.</returns>
        /// <response code="200">Returns matching sections.</response>
        /// <response code="400">If search parameters are invalid.</response>
        [HttpGet("document/{documentId}/search")]
        public async Task<ActionResult<IEnumerable<DocumentSectionDto>>> SearchSections(
            int documentId,
            [FromQuery] string searchTerm,
            [FromQuery] bool fuzzyMatch = true,
            [FromQuery] double similarityThreshold = 0.8,
            [FromQuery] int? parentId = null)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return BadRequest("Search term is required");

            if (similarityThreshold < 0.0 || similarityThreshold > 1.0)
                return BadRequest("Similarity threshold must be between 0.0 and 1.0");

            var results = await _documentSectionService.SearchSectionsAsync(documentId, searchTerm, fuzzyMatch, similarityThreshold);
            
            // Filter by parent if specified
            if (parentId.HasValue)
            {
                results = results.Where(s => s.ParentSectionId == parentId.Value).ToList();
            }

            return Ok(results);
        }

        /// <summary>
        /// Finds potential duplicate sections for a given title within a document.
        /// Useful for preventing duplicate section creation during imports or manual entry.
        /// </summary>
        /// <param name="documentId">The ID of the document to search within.</param>
        /// <param name="title">The section title to check for duplicates.</param>
        /// <param name="similarityThreshold">Minimum similarity score to consider a duplicate (0.0-1.0, default: 0.8).</param>
        /// <param name="parentId">Optional parent section ID to limit search scope.</param>
        /// <returns>A list of potential duplicate sections ordered by similarity.</returns>
        /// <response code="200">Returns potential duplicates (empty list if none found).</response>
        /// <response code="400">If parameters are invalid.</response>
        [HttpGet("document/{documentId}/duplicates")]
        public async Task<ActionResult<IEnumerable<DocumentSectionDto>>> FindPotentialDuplicates(
            int documentId,
            [FromQuery] string title,
            [FromQuery] double similarityThreshold = 0.8,
            [FromQuery] int? parentId = null)
        {
            if (string.IsNullOrWhiteSpace(title))
                return BadRequest("Title is required");

            if (similarityThreshold < 0.0 || similarityThreshold > 1.0)
                return BadRequest("Similarity threshold must be between 0.0 and 1.0");

            var duplicates = await _documentSectionService.FindPotentialDuplicatesAsync(documentId, title, similarityThreshold);
            
            // Filter by parent if specified
            if (parentId.HasValue)
            {
                duplicates = duplicates.Where(s => s.ParentSectionId == parentId.Value).ToList();
            }

            return Ok(duplicates);
        }

        /// <summary>
        /// Finds the best matching existing section for import validation.
        /// Returns the single best match or null if no suitable match is found.
        /// </summary>
        /// <param name="documentId">The ID of the document to search within.</param>
        /// <param name="title">The section title to find a match for.</param>
        /// <param name="parentId">Optional parent section ID for hierarchical matching.</param>
        /// <param name="similarityThreshold">Minimum similarity score to consider a match (0.0-1.0, default: 0.9).</param>
        /// <returns>The best matching section or null if no match found.</returns>
        /// <response code="200">Returns the best match or null.</response>
        /// <response code="400">If parameters are invalid.</response>
        [HttpGet("document/{documentId}/find-existing")]
        public async Task<ActionResult<DocumentSectionDto?>> FindExistingSection(
            int documentId,
            [FromQuery] string title,
            [FromQuery] int? parentId = null,
            [FromQuery] double similarityThreshold = 0.9)
        {
            if (string.IsNullOrWhiteSpace(title))
                return BadRequest("Title is required");

            if (similarityThreshold < 0.0 || similarityThreshold > 1.0)
                return BadRequest("Similarity threshold must be between 0.0 and 1.0");

            var existingSection = await _documentSectionService.FindExistingSectionAsync(documentId, title, parentId, similarityThreshold);
            return Ok(existingSection);
        }

        #endregion
    }

    #region Request Models

    /// <summary>
    /// Request model for moving a section.
    /// </summary>
    public class MoveSectionRequest
    {
        /// <summary>
        /// The new parent section ID (null for document root).
        /// </summary>
        public int? NewParentId { get; set; }

        /// <summary>
        /// The new order position (null to append).
        /// </summary>
        public int? NewOrder { get; set; }
    }

    /// <summary>
    /// Request model for reordering sections.
    /// </summary>
    public class ReorderSectionsRequest
    {
        /// <summary>
        /// The parent section ID (null for document root).
        /// </summary>
        public int? ParentId { get; set; }

        /// <summary>
        /// The list of section IDs in the desired order.
        /// </summary>
        public List<int> SectionIds { get; set; } = new();
    }

    #endregion
}