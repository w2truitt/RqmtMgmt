using RqmtMgmtShared;
using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    /// <summary>
    /// API controller for managing test suites with full CRUD operations.
    /// Provides endpoints for creating, reading, updating, and deleting test suite collections.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class TestSuiteController : ControllerBase
    {
        private readonly RqmtMgmtShared.ITestSuiteService _testSuiteService;

        /// <summary>
        /// Initializes a new instance of the TestSuiteController with the specified test suite service.
        /// </summary>
        /// <param name="testSuiteService">The service for test suite operations.</param>
        public TestSuiteController(RqmtMgmtShared.ITestSuiteService testSuiteService)
        {
            _testSuiteService = testSuiteService;
        }

        /// <summary>
        /// Retrieves all test suites from the system.
        /// </summary>
        /// <returns>A list of all test suites.</returns>
        /// <response code="200">Returns the list of test suites.</response>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TestSuiteDto>>> GetAll()
        {
            var suites = await _testSuiteService.GetAllAsync();
            return Ok(suites);
        }

        /// <summary>
        /// Retrieves a specific test suite by its ID.
        /// </summary>
        /// <param name="id">The unique identifier of the test suite.</param>
        /// <returns>The test suite if found.</returns>
        /// <response code="200">Returns the requested test suite.</response>
        /// <response code="404">If the test suite is not found.</response>
        [HttpGet("{id}")]
        public async Task<ActionResult<TestSuiteDto>> GetById(int id)
        {
            var suite = await _testSuiteService.GetByIdAsync(id);
            if (suite == null) return NotFound();
            return Ok(suite);
        }

        /// <summary>
        /// Creates a new test suite in the system.
        /// </summary>
        /// <param name="dto">The test suite data to create.</param>
        /// <returns>The created test suite with its assigned ID.</returns>
        /// <response code="201">Returns the newly created test suite.</response>
        /// <response code="400">If the test suite data is invalid or creation fails.</response>
        [HttpPost]
        public async Task<ActionResult<TestSuiteDto>> Create([FromBody] TestSuiteDto dto)
        {
            var created = await _testSuiteService.CreateAsync(dto);
            if (created == null) return BadRequest();
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        /// <summary>
        /// Updates an existing test suite with new data.
        /// </summary>
        /// <param name="id">The ID of the test suite to update.</param>
        /// <param name="dto">The updated test suite data.</param>
        /// <returns>No content if successful.</returns>
        /// <response code="204">If the test suite was successfully updated.</response>
        /// <response code="400">If the ID in the URL doesn't match the ID in the request body.</response>
        /// <response code="404">If the test suite is not found.</response>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] TestSuiteDto dto)
        {
            if (id != dto.Id) return BadRequest();
            var success = await _testSuiteService.UpdateAsync(dto);
            if (!success) return NotFound();
            return NoContent();
        }

        /// <summary>
        /// Deletes a test suite from the system. Associated test cases will have their SuiteId set to null.
        /// </summary>
        /// <param name="id">The ID of the test suite to delete.</param>
        /// <returns>No content if successful.</returns>
        /// <response code="204">If the test suite was successfully deleted.</response>
        /// <response code="404">If the test suite is not found.</response>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _testSuiteService.DeleteAsync(id);
            if (!deleted) return NotFound();
            return NoContent();
        }
    }
}