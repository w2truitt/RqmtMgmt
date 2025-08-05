using RqmtMgmtShared;
using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    /// <summary>
    /// API controller for managing test cases.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class TestCaseController : ControllerBase
    {
        private readonly ITestCaseService _testCaseService;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestCaseController"/> class.
        /// </summary>
        public TestCaseController(ITestCaseService testCaseService)
        {
            _testCaseService = testCaseService;
        }

        /// <summary>
        /// Gets all test cases.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TestCaseDto>>> GetAll()
        {
            var cases = await _testCaseService.GetAllAsync();
            var dtos = cases.Select(ToDto);
            return Ok(dtos);
        }

        /// <summary>
        /// Gets a test case by its unique identifier.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<TestCaseDto>> GetById(int id)
        {
            var testCase = await _testCaseService.GetByIdAsync(id);
            if (testCase == null) return NotFound();
            return Ok(ToDto(testCase));
        }

        /// <summary>
        /// Creates a new test case.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<TestCaseDto>> Create([FromBody] TestCaseDto dto)
        {
            var model = FromDto(dto);
            var created = await _testCaseService.CreateAsync(model);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, ToDto(created));
        }

        /// <summary>
        /// Updates an existing test case.
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<TestCaseDto>> Update(int id, [FromBody] TestCaseDto dto)
        {
            if (id != dto.Id) return BadRequest();
            var existing = await _testCaseService.GetByIdAsync(id);
            if (existing == null) return NotFound();
            var updated = await _testCaseService.UpdateAsync(FromDto(dto));
            return Ok(ToDto(updated));
        }

        /// <summary>
        /// Deletes a test case by its unique identifier.
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _testCaseService.DeleteAsync(id);
            if (!deleted) return NotFound();
            return NoContent();
        }

        // Mapping helpers
        private static TestCaseDto ToDto(TestCase tc) => new TestCaseDto
        {
            Id = tc.Id,
            SuiteId = tc.SuiteId,
            Title = tc.Title,
            Description = tc.Description,
            Steps = tc.Steps != null
                ? tc.Steps.Select(s => new TestStepDto
                {
                    Description = s.Description,
                    ExpectedResult = s.ExpectedResult
                }).ToList()
                : new List<TestStepDto>(),
            CreatedBy = tc.CreatedBy,
            CreatedAt = tc.CreatedAt
        };
        // Ensures TestStep navigation property is set for EF tracking
        private static TestCase FromDto(TestCaseDto dto)
        {
            var testCase = new TestCase
            {
                Id = dto.Id,
                SuiteId = dto.SuiteId,
                Title = dto.Title,
                Description = dto.Description,
                CreatedBy = dto.CreatedBy,
                CreatedAt = dto.CreatedAt,
                Steps = new List<TestStep>()
            };
            if (dto.Steps != null)
            {
                foreach (var s in dto.Steps)
                {
                    var step = new TestStep
                    {
                        Description = s.Description,
                        ExpectedResult = s.ExpectedResult,
                        TestCase = testCase
                    };
                    testCase.Steps.Add(step);
                }
            }
            return testCase;
        }

        // Ensure that after mapping, all TestStep objects are added to the TestCase.Steps collection before saving the TestCase. EF Core will cascade insert if relationship is configured. If not, a post-processing step may be needed.

        /// <summary>
        /// Adds a step to a test case.
        /// </summary>
        [HttpPost("{id}/steps")]
        public async Task<ActionResult<TestStepDto>> AddStep(int id, [FromBody] TestStepDto dto)
        {
            var testStep = new TestStep { Description = dto.Description, ExpectedResult = dto.ExpectedResult };
            var added = await _testCaseService.AddStepAsync(id, testStep);
            return Ok(new TestStepDto { Description = added.Description, ExpectedResult = added.ExpectedResult });
        }

        /// <summary>
        /// Removes a step from a test case.
        /// </summary>
        [HttpDelete("{id}/steps/{stepId}")]
        public async Task<IActionResult> RemoveStep(int id, int stepId)
        {
            var removed = await _testCaseService.RemoveStepAsync(id, stepId);
            if (!removed) return NotFound();
            return NoContent();
        }
    }
}
