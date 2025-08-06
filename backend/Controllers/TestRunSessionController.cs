using Microsoft.AspNetCore.Mvc;
using RqmtMgmtShared;

namespace backend.Controllers
{
    /// <summary>
    /// API controller for test run session management operations
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class TestRunSessionController : ControllerBase
    {
        private readonly ITestRunSessionService _testRunSessionService;

        public TestRunSessionController(ITestRunSessionService testRunSessionService)
        {
            _testRunSessionService = testRunSessionService;
        }

        /// <summary>
        /// Get all test run sessions
        /// </summary>
        /// <returns>List of all test run sessions</returns>
        [HttpGet]
        public async Task<ActionResult<List<TestRunSessionDto>>> GetAll()
        {
            try
            {
                var sessions = await _testRunSessionService.GetAllAsync();
                return Ok(sessions);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving test run sessions.", error = ex.Message });
            }
        }

        /// <summary>
        /// Get a test run session by ID
        /// </summary>
        /// <param name="id">The test run session ID</param>
        /// <returns>The test run session if found</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<TestRunSessionDto>> GetById(int id)
        {
            try
            {
                var session = await _testRunSessionService.GetByIdAsync(id);
                if (session == null)
                {
                    return NotFound(new { message = $"Test run session with ID {id} was not found." });
                }
                return Ok(session);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving the test run session.", error = ex.Message });
            }
        }

        /// <summary>
        /// Create a new test run session
        /// </summary>
        /// <param name="testRunSession">The test run session to create</param>
        /// <returns>The created test run session</returns>
        [HttpPost]
        public async Task<ActionResult<TestRunSessionDto>> Create([FromBody] TestRunSessionDto testRunSession)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var createdSession = await _testRunSessionService.CreateAsync(testRunSession);
                if (createdSession == null)
                {
                    return StatusCode(500, new { message = "Failed to create test run session." });
                }

                return CreatedAtAction(nameof(GetById), new { id = createdSession.Id }, createdSession);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the test run session.", error = ex.Message });
            }
        }

        /// <summary>
        /// Update an existing test run session
        /// </summary>
        /// <param name="id">The test run session ID</param>
        /// <param name="testRunSession">The updated test run session data</param>
        /// <returns>Success or error response</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] TestRunSessionDto testRunSession)
        {
            try
            {
                if (id != testRunSession.Id)
                {
                    return BadRequest(new { message = "ID in URL does not match ID in request body." });
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var success = await _testRunSessionService.UpdateAsync(testRunSession);
                if (!success)
                {
                    return NotFound(new { message = $"Test run session with ID {id} was not found." });
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the test run session.", error = ex.Message });
            }
        }

        /// <summary>
        /// Delete a test run session
        /// </summary>
        /// <param name="id">The test run session ID</param>
        /// <returns>Success or error response</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var success = await _testRunSessionService.DeleteAsync(id);
                if (!success)
                {
                    return NotFound(new { message = $"Test run session with ID {id} was not found." });
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting the test run session.", error = ex.Message });
            }
        }

        /// <summary>
        /// Start a new test run session
        /// </summary>
        /// <param name="testRunSession">The test run session to start</param>
        /// <returns>The started test run session</returns>
        [HttpPost("start")]
        public async Task<ActionResult<TestRunSessionDto>> StartTestRunSession([FromBody] TestRunSessionDto testRunSession)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var startedSession = await _testRunSessionService.StartTestRunSessionAsync(testRunSession);
                if (startedSession == null)
                {
                    return StatusCode(500, new { message = "Failed to start test run session." });
                }

                return CreatedAtAction(nameof(GetById), new { id = startedSession.Id }, startedSession);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while starting the test run session.", error = ex.Message });
            }
        }

        /// <summary>
        /// Complete a test run session
        /// </summary>
        /// <param name="id">The test run session ID</param>
        /// <returns>Success or error response</returns>
        [HttpPost("{id}/complete")]
        public async Task<IActionResult> CompleteTestRunSession(int id)
        {
            try
            {
                var success = await _testRunSessionService.CompleteTestRunSessionAsync(id);
                if (!success)
                {
                    return NotFound(new { message = $"Test run session with ID {id} was not found." });
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while completing the test run session.", error = ex.Message });
            }
        }

        /// <summary>
        /// Abort a test run session
        /// </summary>
        /// <param name="id">The test run session ID</param>
        /// <returns>Success or error response</returns>
        [HttpPost("{id}/abort")]
        public async Task<IActionResult> AbortTestRunSession(int id)
        {
            try
            {
                var success = await _testRunSessionService.AbortTestRunSessionAsync(id);
                if (!success)
                {
                    return NotFound(new { message = $"Test run session with ID {id} was not found." });
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while aborting the test run session.", error = ex.Message });
            }
        }

        /// <summary>
        /// Get active test run sessions
        /// </summary>
        /// <returns>List of active test run sessions</returns>
        [HttpGet("active")]
        public async Task<ActionResult<List<TestRunSessionDto>>> GetActiveSessions()
        {
            try
            {
                var sessions = await _testRunSessionService.GetActiveSessionsAsync();
                return Ok(sessions);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving active test run sessions.", error = ex.Message });
            }
        }
    }
}