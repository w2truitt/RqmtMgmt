using Microsoft.AspNetCore.Mvc;
using RqmtMgmtShared;

namespace backend.Controllers
{
    /// <summary>
    /// API controller for managing projects and project team members.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ProjectsController : ControllerBase
    {
        private readonly IProjectService _projectService;
        private readonly IRequirementService _requirementService;

        public ProjectsController(IProjectService projectService, IRequirementService requirementService)
        {
            _projectService = projectService;
            _requirementService = requirementService;
        }

        /// <summary>
        /// Gets a paginated list of projects based on filter criteria.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<PagedResult<ProjectDto>>> GetProjects([FromQuery] ProjectFilterDto filter)
        {
            try
            {
                var result = await _projectService.GetProjectsAsync(filter);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets a project by its ID.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ProjectDto>> GetProject(int id)
        {
            try
            {
                var project = await _projectService.GetProjectByIdAsync(id);
                if (project == null)
                {
                    return NotFound($"Project with ID {id} not found.");
                }
                return Ok(project);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets a project by its code.
        /// </summary>
        [HttpGet("by-code/{code}")]
        public async Task<ActionResult<ProjectDto>> GetProjectByCode(string code)
        {
            try
            {
                var project = await _projectService.GetProjectByCodeAsync(code);
                if (project == null)
                {
                    return NotFound($"Project with code '{code}' not found.");
                }
                return Ok(project);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Creates a new project.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ProjectDto>> CreateProject([FromBody] CreateProjectDto createProjectDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var project = await _projectService.CreateProjectAsync(createProjectDto);
                if (project == null)
                {
                    return BadRequest("Failed to create project.");
                }
                return CreatedAtAction(nameof(GetProject), new { id = project.Id }, project);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Updates an existing project.
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<ProjectDto>> UpdateProject(int id, [FromBody] UpdateProjectDto updateProjectDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var project = await _projectService.UpdateProjectAsync(id, updateProjectDto);
                if (project == null)
                {
                    return NotFound($"Project with ID {id} not found.");
                }
                return Ok(project);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Deletes a project and all its associated data.
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteProject(int id)
        {
            try
            {
                var success = await _projectService.DeleteProjectAsync(id);
                if (!success)
                {
                    return NotFound($"Project with ID {id} not found.");
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets all team members for a specific project.
        /// </summary>
        [HttpGet("{id}/team")]
        public async Task<ActionResult<List<ProjectTeamMemberDto>>> GetProjectTeamMembers(int id)
        {
            try
            {
                // Verify project exists first
                var project = await _projectService.GetProjectByIdAsync(id);
                if (project == null)
                {
                    return NotFound($"Project with ID {id} not found.");
                }

                var teamMembers = await _projectService.GetProjectTeamMembersAsync(id);
                return Ok(teamMembers);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Adds a user to a project team with a specific role.
        /// </summary>
        [HttpPost("{id}/team")]
        public async Task<ActionResult<ProjectTeamMemberDto>> AddTeamMember(int id, [FromBody] AddProjectTeamMemberDto addTeamMemberDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var teamMember = await _projectService.AddTeamMemberAsync(id, addTeamMemberDto);
                if (teamMember == null)
                {
                    return NotFound($"Project with ID {id} not found or user with ID {addTeamMemberDto.UserId} not found.");
                }
                return Ok(teamMember);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Updates a team member's role or status in a project.
        /// </summary>
        [HttpPut("{projectId}/team/{userId}")]
        public async Task<ActionResult<ProjectTeamMemberDto>> UpdateTeamMember(int projectId, int userId, [FromBody] UpdateProjectTeamMemberDto updateTeamMemberDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var teamMember = await _projectService.UpdateTeamMemberAsync(projectId, userId, updateTeamMemberDto);
                if (teamMember == null)
                {
                    return NotFound($"Team member not found in project {projectId}.");
                }
                return Ok(teamMember);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Removes a user from a project team.
        /// </summary>
        [HttpDelete("{projectId}/team/{userId}")]
        public async Task<ActionResult> RemoveTeamMember(int projectId, int userId)
        {
            try
            {
                var success = await _projectService.RemoveTeamMemberAsync(projectId, userId);
                if (!success)
                {
                    return NotFound($"Team member not found in project {projectId}.");
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets all projects that a user is a member of.
        /// </summary>
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<List<ProjectDto>>> GetUserProjects(int userId)
        {
            try
            {
                var projects = await _projectService.GetUserProjectsAsync(userId);
                return Ok(projects);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets requirements for a specific project.
        /// </summary>
        [HttpGet("{id}/requirements")]
        public async Task<ActionResult<PagedResult<RequirementDto>>> GetProjectRequirements(
            int id, 
            [FromQuery] int page = 1, 
            [FromQuery] int pageSize = 20,
            [FromQuery] string? searchTerm = null,
            [FromQuery] string? sortBy = null,
            [FromQuery] bool sortDescending = false)
        {
            try
            {
                // Verify project exists
                var project = await _projectService.GetProjectByIdAsync(id);
                if (project == null)
                {
                    return NotFound($"Project with ID {id} not found.");
                }

                // Create pagination parameters with project filtering
                var parameters = new PaginationParameters
                {
                    PageNumber = page,
                    PageSize = pageSize,
                    SearchTerm = searchTerm,
                    SortBy = sortBy,
                    SortDescending = sortDescending,
                    ProjectId = id
                };

                // Get filtered requirements for this project
                var result = await _requirementService.GetPagedAsync(parameters);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets test suites for a specific project.
        /// </summary>
        [HttpGet("{id}/test-suites")]
        public Task<ActionResult<PagedResult<TestSuiteDto>>> GetProjectTestSuites(int id, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                // This would need to be implemented in a TestSuiteService with project filtering
                // For now, return a placeholder
                return Task.FromResult<ActionResult<PagedResult<TestSuiteDto>>>(Ok(new PagedResult<TestSuiteDto>
                {
                    Items = new List<TestSuiteDto>(),
                    TotalItems = 0,
                    PageNumber = page,
                    PageSize = pageSize
                }));
            }
            catch (Exception ex)
            {
                return Task.FromResult<ActionResult<PagedResult<TestSuiteDto>>>(StatusCode(500, $"Internal server error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Gets test plans for a specific project.
        /// </summary>
        [HttpGet("{id}/test-plans")]
        public Task<ActionResult<PagedResult<TestPlanDto>>> GetProjectTestPlans(int id, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                // This would need to be implemented in a TestPlanService with project filtering
                // For now, return a placeholder
                return Task.FromResult<ActionResult<PagedResult<TestPlanDto>>>(Ok(new PagedResult<TestPlanDto>
                {
                    Items = new List<TestPlanDto>(),
                    TotalItems = 0,
                    PageNumber = page,
                    PageSize = pageSize
                }));
            }
            catch (Exception ex)
            {
                return Task.FromResult<ActionResult<PagedResult<TestPlanDto>>>(StatusCode(500, $"Internal server error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Generates the next requirement ID for a project.
        /// </summary>
        [HttpGet("{id}/next-requirement-id")]
        public async Task<ActionResult<string>> GetNextRequirementId(int id)
        {
            try
            {
                var nextId = await _projectService.GenerateNextRequirementIdAsync(id);
                return Ok(nextId);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
