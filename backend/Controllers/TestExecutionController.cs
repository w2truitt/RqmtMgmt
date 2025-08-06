using Microsoft.AspNetCore.Mvc;
using RqmtMgmtShared;

namespace backend.Controllers
{
    /// <summary>
    /// API controller for test execution and results tracking
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class TestExecutionController : ControllerBase
    {
        private readonly ITestExecutionService _testExecutionService;

        public TestExecutionController(ITestExecutionService testExecutionService)
        {
            _testExecutionService = testExecutionService;
        }

        /// <summary>
        /// Execute a test case and record results
        /// </summary>
        /// <param name="execution">The test case execution data</param>
        /// <returns>The executed test case result</returns>
        [HttpPost("execute-testcase")]
        public async Task<ActionResult<TestCaseExecutionDto>> ExecuteTestCase([FromBody] TestCaseExecutionDto execution)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _testExecutionService.ExecuteTestCaseAsync(execution);
                if (result == null)
                {
                    return StatusCode(500, new { message = "Failed to execute test case." });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while executing the test case.", error = ex.Message });
            }
        }

        /// <summary>
        /// Update test case execution results
        /// </summary>
        /// <param name="id">The test case execution ID</param>
        /// <param name="execution">The updated execution data</param>
        /// <returns>Success or error response</returns>
        [HttpPut("testcase-execution/{id}")]
        public async Task<IActionResult> UpdateTestCaseExecution(int id, [FromBody] TestCaseExecutionDto execution)
        {
            try
            {
                if (id != execution.Id)
                {
                    return BadRequest(new { message = "ID in URL does not match ID in request body." });
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var success = await _testExecutionService.UpdateTestCaseExecutionAsync(execution);
                if (!success)
                {
                    return NotFound(new { message = $"Test case execution with ID {id} was not found." });
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the test case execution.", error = ex.Message });
            }
        }

        /// <summary>
        /// Update individual test step result
        /// </summary>
        /// <param name="stepExecution">The test step execution data</param>
        /// <returns>The updated test step result</returns>
        [HttpPost("update-step-result")]
        public async Task<ActionResult<TestStepExecutionDto>> UpdateStepResult([FromBody] TestStepExecutionDto stepExecution)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _testExecutionService.UpdateStepResultAsync(stepExecution);
                if (result == null)
                {
                    return StatusCode(500, new { message = "Failed to update test step result." });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the test step result.", error = ex.Message });
            }
        }

        /// <summary>
        /// Get all test case executions for a test run session
        /// </summary>
        /// <param name="sessionId">The test run session ID</param>
        /// <returns>List of test case executions</returns>
        [HttpGet("session/{sessionId}/executions")]
        public async Task<ActionResult<List<TestCaseExecutionDto>>> GetExecutionsForSession(int sessionId)
        {
            try
            {
                var executions = await _testExecutionService.GetExecutionsForSessionAsync(sessionId);
                return Ok(executions);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving test case executions.", error = ex.Message });
            }
        }

        /// <summary>
        /// Get all test step executions for a test case execution
        /// </summary>
        /// <param name="caseExecutionId">The test case execution ID</param>
        /// <returns>List of test step executions</returns>
        [HttpGet("case-execution/{caseExecutionId}/steps")]
        public async Task<ActionResult<List<TestStepExecutionDto>>> GetStepExecutionsForCase(int caseExecutionId)
        {
            try
            {
                var stepExecutions = await _testExecutionService.GetStepExecutionsForCaseAsync(caseExecutionId);
                return Ok(stepExecutions);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving test step executions.", error = ex.Message });
            }
        }

        /// <summary>
        /// Get overall test execution statistics
        /// </summary>
        /// <returns>Test execution statistics</returns>
        [HttpGet("statistics")]
        public async Task<ActionResult<TestExecutionStatsDto>> GetExecutionStats()
        {
            try
            {
                var stats = await _testExecutionService.GetExecutionStatsAsync();
                return Ok(stats);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving test execution statistics.", error = ex.Message });
            }
        }

        /// <summary>
        /// Get test execution statistics for a specific session
        /// </summary>
        /// <param name="sessionId">The test run session ID</param>
        /// <returns>Test execution statistics for the session</returns>
        [HttpGet("session/{sessionId}/statistics")]
        public async Task<ActionResult<TestExecutionStatsDto>> GetExecutionStatsForSession(int sessionId)
        {
            try
            {
                var stats = await _testExecutionService.GetExecutionStatsForSessionAsync(sessionId);
                return Ok(stats);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving session test execution statistics.", error = ex.Message });
            }
        }
    }
}