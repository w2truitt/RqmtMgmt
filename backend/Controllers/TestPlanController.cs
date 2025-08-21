using RqmtMgmtShared;
using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace backend.Controllers
{
    /// <summary>
    /// API controller for managing test plans with full CRUD operations.
    /// Provides endpoints for creating, reading, updating, and deleting test plan documents.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TestPlanController : ControllerBase
    {
        private readonly RqmtMgmtShared.ITestPlanService _testPlanService;

        /// <summary>
        /// Initializes a new instance of the TestPlanController with the specified test plan service.
        /// </summary>
        /// <param name="testPlanService">The service for test plan operations.</param>
        public TestPlanController(RqmtMgmtShared.ITestPlanService testPlanService)
        {
            _testPlanService = testPlanService;
        }

        /// <summary>
        /// Retrieves all test plans from the system.
        /// </summary>
        /// <returns>A list of all test plans with their type information.</returns>
        /// <response code="200">Returns the list of test plans.</response>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TestPlanDto>>> GetAll()
        {
            var plans = await _testPlanService.GetAllAsync();
            return Ok(plans);
        }

        /// <summary>
        /// Retrieves a specific test plan by its ID.
        /// </summary>
        /// <param name="id">The unique identifier of the test plan.</param>
        /// <returns>The test plan if found, including its type and metadata.</returns>
        /// <response code="200">Returns the requested test plan.</response>
        /// <response code="404">If the test plan is not found.</response>
        [HttpGet("{id}")]
        public async Task<ActionResult<TestPlanDto>> GetById(int id)
        {
            var plan = await _testPlanService.GetByIdAsync(id);
            if (plan == null) return NotFound();
            return Ok(plan);
        }

        /// <summary>
        /// Creates a new test plan in the system with type validation.
        /// </summary>
        /// <param name="dto">The test plan data to create, including type specification.</param>
        /// <returns>The created test plan with its assigned ID.</returns>
        /// <response code="201">Returns the newly created test plan.</response>
        /// <response code="400">If the test plan data is invalid or creation fails.</response>
        [HttpPost]
        public async Task<ActionResult<TestPlanDto>> Create([FromBody] TestPlanDto dto)
        {
            var created = await _testPlanService.CreateAsync(dto);
            if (created == null) return BadRequest();
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        /// <summary>
        /// Updates an existing test plan with new data including type changes.
        /// </summary>
        /// <param name="id">The ID of the test plan to update.</param>
        /// <param name="dto">The updated test plan data.</param>
        /// <returns>No content if successful.</returns>
        /// <response code="204">If the test plan was successfully updated.</response>
        /// <response code="400">If the ID in the URL doesn't match the ID in the request body.</response>
        /// <response code="404">If the test plan is not found.</response>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] TestPlanDto dto)
        {
            if (id != dto.Id) return BadRequest();
            var success = await _testPlanService.UpdateAsync(dto);
            if (!success) return NotFound();
            return NoContent();
        }

        /// <summary>
        /// Deletes a test plan from the system. Associated test case links are cascade deleted.
        /// </summary>
        /// <param name="id">The ID of the test plan to delete.</param>
        /// <returns>No content if successful.</returns>
        /// <response code="204">If the test plan was successfully deleted.</response>
        /// <response code="404">If the test plan is not found.</response>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _testPlanService.DeleteAsync(id);
            if (!deleted) return NotFound();
            return NoContent();
        }
    }
}