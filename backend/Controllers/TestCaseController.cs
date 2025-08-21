using RqmtMgmtShared;
using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace backend.Controllers
{
    /// <summary>
    /// API controller for managing test cases with full CRUD operations.
    /// Provides endpoints for creating, reading, updating, and deleting test cases and their associated test steps.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TestCaseController : ControllerBase
    {
        private readonly RqmtMgmtShared.ITestCaseService _testCaseService;

        /// <summary>
        /// Initializes a new instance of the TestCaseController with the specified test case service.
        /// </summary>
        /// <param name="testCaseService">The service for test case operations.</param>
        public TestCaseController(RqmtMgmtShared.ITestCaseService testCaseService)
        {
            _testCaseService = testCaseService;
        }

        /// <summary>
        /// Retrieves all test cases from the system including their test steps.
        /// </summary>
        /// <returns>A list of all test cases with their associated test steps.</returns>
        /// <response code="200">Returns the list of test cases.</response>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TestCaseDto>>> GetAll()
        {
            var cases = await _testCaseService.GetAllAsync();
            return Ok(cases);
        }

        /// <summary>
        /// Retrieves a specific test case by its ID including associated test steps.
        /// </summary>
        /// <param name="id">The unique identifier of the test case.</param>
        /// <returns>The test case if found, including its test steps.</returns>
        /// <response code="200">Returns the requested test case.</response>
        /// <response code="404">If the test case is not found.</response>
        [HttpGet("{id}")]
        public async Task<ActionResult<TestCaseDto>> GetById(int id)
        {
            var testCase = await _testCaseService.GetByIdAsync(id);
            if (testCase == null) return NotFound();
            return Ok(testCase);
        }

        /// <summary>
        /// Creates a new test case in the system with validation for required fields and test steps.
        /// </summary>
        /// <param name="dto">The test case data to create, including test steps.</param>
        /// <returns>The created test case with its assigned ID.</returns>
        /// <response code="201">Returns the newly created test case.</response>
        /// <response code="400">If the test case data is invalid or creation fails.</response>
        [HttpPost]
        public async Task<ActionResult<TestCaseDto>> Create([FromBody] TestCaseDto dto)
        {
            var created = await _testCaseService.CreateAsync(dto);
            if (created == null) return BadRequest();
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        /// <summary>
        /// Updates an existing test case with new data, replacing all associated test steps.
        /// </summary>
        /// <param name="id">The ID of the test case to update.</param>
        /// <param name="dto">The updated test case data, including test steps.</param>
        /// <returns>No content if successful.</returns>
        /// <response code="204">If the test case was successfully updated.</response>
        /// <response code="400">If the ID in the URL doesn't match the ID in the request body.</response>
        /// <response code="404">If the test case is not found.</response>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] TestCaseDto dto)
        {
            if (id != dto.Id) return BadRequest();
            var success = await _testCaseService.UpdateAsync(dto);
            if (!success) return NotFound();
            return NoContent();
        }

        /// <summary>
        /// Deletes a test case from the system. Associated test steps are automatically deleted.
        /// </summary>
        /// <param name="id">The ID of the test case to delete.</param>
        /// <returns>No content if successful.</returns>
        /// <response code="204">If the test case was successfully deleted.</response>
        /// <response code="404">If the test case is not found.</response>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _testCaseService.DeleteAsync(id);
            if (!deleted) return NotFound();
            return NoContent();
        }
    }
}