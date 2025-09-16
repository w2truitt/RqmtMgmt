using backend.Services;
using Microsoft.AspNetCore.Mvc;
using RqmtMgmtShared;

namespace backend.Controllers
{
    /// <summary>
    /// API controller for managing requirement traceability links.
    /// Provides endpoints for creating, reading, and deleting trace relationships between requirements.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class RequirementTracesController : ControllerBase
    {
        private readonly IRequirementTraceService _requirementTraceService;

        /// <summary>
        /// Initializes a new instance of the RequirementTracesController with the specified service.
        /// </summary>
        /// <param name="requirementTraceService">The service for requirement trace operations.</param>
        public RequirementTracesController(IRequirementTraceService requirementTraceService)
        {
            _requirementTraceService = requirementTraceService;
        }

        /// <summary>
        /// Retrieves all trace links where the specified requirement is the source.
        /// </summary>
        /// <param name="sourceRequirementId">The ID of the source requirement.</param>
        /// <returns>A list of trace links originating from the specified requirement.</returns>
        /// <response code="200">Returns the list of outgoing trace links.</response>
        [HttpGet("source/{sourceRequirementId}")]
        public async Task<ActionResult<IEnumerable<RequirementTraceDto>>> GetBySourceRequirementId(int sourceRequirementId)
        {
            var traces = await _requirementTraceService.GetBySourceRequirementIdAsync(sourceRequirementId);
            return Ok(traces);
        }

        /// <summary>
        /// Retrieves all trace links where the specified requirement is the target.
        /// </summary>
        /// <param name="targetRequirementId">The ID of the target requirement.</param>
        /// <returns>A list of trace links pointing to the specified requirement.</returns>
        /// <response code="200">Returns the list of incoming trace links.</response>
        [HttpGet("target/{targetRequirementId}")]
        public async Task<ActionResult<IEnumerable<RequirementTraceDto>>> GetByTargetRequirementId(int targetRequirementId)
        {
            var traces = await _requirementTraceService.GetByTargetRequirementIdAsync(targetRequirementId);
            return Ok(traces);
        }

        /// <summary>
        /// Retrieves the complete trace chain for a requirement (both incoming and outgoing traces).
        /// </summary>
        /// <param name="requirementId">The ID of the requirement.</param>
        /// <returns>A list of all trace links related to the specified requirement.</returns>
        /// <response code="200">Returns the complete trace chain for the requirement.</response>
        [HttpGet("chain/{requirementId}")]
        public async Task<ActionResult<IEnumerable<RequirementTraceDto>>> GetTraceChain(int requirementId)
        {
            var traces = await _requirementTraceService.GetTraceChainAsync(requirementId);
            return Ok(traces);
        }

        /// <summary>
        /// Retrieves a specific requirement trace by its ID.
        /// </summary>
        /// <param name="id">The ID of the requirement trace.</param>
        /// <returns>The requirement trace if found.</returns>
        /// <response code="200">Returns the requirement trace.</response>
        /// <response code="404">If the requirement trace is not found.</response>
        [HttpGet("{id}")]
        public async Task<ActionResult<RequirementTraceDto>> GetById(int id)
        {
            var trace = await _requirementTraceService.GetByIdAsync(id);
            if (trace == null)
                return NotFound();

            return Ok(trace);
        }

        /// <summary>
        /// Creates a new requirement trace link.
        /// </summary>
        /// <param name="trace">The requirement trace to create.</param>
        /// <returns>The created requirement trace with assigned ID.</returns>
        /// <response code="201">Returns the newly created requirement trace.</response>
        /// <response code="400">If the trace data is invalid or violates validation rules.</response>
        [HttpPost]
        public async Task<ActionResult<RequirementTraceDto>> Create([FromBody] RequirementTraceDto trace)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var createdTrace = await _requirementTraceService.CreateAsync(trace);
            if (createdTrace == null)
                return BadRequest("Failed to create requirement trace. Check validation rules.");

            return CreatedAtAction(nameof(GetById), new { id = createdTrace.Id }, createdTrace);
        }

        /// <summary>
        /// Deletes a requirement trace by its ID.
        /// </summary>
        /// <param name="id">The ID of the requirement trace to delete.</param>
        /// <returns>No content if successful.</returns>
        /// <response code="204">If the deletion was successful.</response>
        /// <response code="404">If the requirement trace is not found.</response>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _requirementTraceService.DeleteAsync(id);
            if (!success)
                return NotFound();

            return NoContent();
        }

        /// <summary>
        /// Validates whether a trace link can be created between two requirements.
        /// </summary>
        /// <param name="sourceRequirementId">The ID of the source requirement.</param>
        /// <param name="targetRequirementId">The ID of the target requirement.</param>
        /// <param name="traceType">The type of trace relationship.</param>
        /// <returns>A validation result indicating if the trace is valid.</returns>
        /// <response code="200">Returns the validation result.</response>
        [HttpGet("validate")]
        public async Task<ActionResult<bool>> ValidateTrace(
            [FromQuery] int sourceRequirementId,
            [FromQuery] int targetRequirementId,
            [FromQuery] TraceType traceType)
        {
            var isValid = await _requirementTraceService.ValidateTraceAsync(sourceRequirementId, targetRequirementId, traceType);
            return Ok(isValid);
        }
    }
}