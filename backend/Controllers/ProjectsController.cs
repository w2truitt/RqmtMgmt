using Microsoft.AspNetCore.Mvc;
using RqmtMgmtShared;
using backend.Data;

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
        private readonly ITestSuiteService _testSuiteService;
        private readonly ITestPlanService _testPlanService;
        private readonly ITestCaseService _testCaseService;

        public ProjectsController(
            IProjectService projectService, 
            IRequirementService requirementService,
            ITestSuiteService testSuiteService,
            ITestPlanService testPlanService,
            ITestCaseService testCaseService)
        {
            _projectService = projectService;
            _requirementService = requirementService;
            _testSuiteService = testSuiteService;
            _testPlanService = testPlanService;
                _testCaseService = testCaseService;
        }

        /// <summary>
        /// Gets a paginated list of projects based on filter criteria.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<PagedResult<ProjectDto>>> GetProjects([FromQuery] ProjectFilterDto filter)
        {
            try
            {
                // Extract current user ID from JWT token for
                // UserIsMember filter
                if (filter.UserIsMember.HasValue && filter.UserIsMember.Value)
                {
                    var currentUserId = GetCurrentUserId();
                    if (currentUserId.HasValue)
                    {
			Console.WriteLine($"[DEBUG] GetProjects - Setting CurrentUserId to: {currentUserId.Value}");
                        filter.CurrentUserId = currentUserId.Value;
                    }
                    else
                    {
                        // If UserIsMember is requested but no valid
                        // user ID found, return empty result
			Console.WriteLine("[DEBUG] GetProjects - No valid user ID found, returning empty result");
                        return Ok(new PagedResult<ProjectDto>
                        {
                            Items = new List<ProjectDto>(),
                            TotalItems = 0,
                            PageNumber = filter.Page,
                            PageSize = filter.PageSize
                        });
                    }
                }

                var result = await _projectService.GetProjectsAsync(filter);
		Console.WriteLine($"[DEBUG] GetProjects - Returning {result.Items.Count} projects (Total: {result.TotalItems})");
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
        /// Extracts the current user ID from the JWT token claims.
        /// </summary>
        /// <returns>The current user ID if found and valid; otherwise, null.</returns>
	private int? GetCurrentUserId()
	{
	    // Enhanced debugging for user ID extraction from JWT
	    // token claims
	    var allClaims = User.Claims.Select(c => $"{c.Type}={c.Value}").ToList();
	    Console.WriteLine($"[DEBUG] GetCurrentUserId - All available claims: {string.Join(", ", allClaims)}");
    
	    // Try to get user ID from 'sub' claim (standard JWT claim
	    // for subject)
	    var userIdClaim = User.FindFirst("sub") ?? User.FindFirst("user_id") ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
    
	    if (userIdClaim != null)
	    {
		Console.WriteLine($"[DEBUG] GetCurrentUserId - Found user ID claim: {userIdClaim.Type}={userIdClaim.Value}");
        
		// First try: Parse as integer (for tokens that
		// already include integer user IDs)
		if (int.TryParse(userIdClaim.Value, out var userId))
		{
		    Console.WriteLine($"[DEBUG] GetCurrentUserId - Successfully parsed user ID: {userId}");
		    return userId;
		}
		else
		{
		    Console.WriteLine($"[DEBUG] GetCurrentUserId - Failed to parse user ID claim value: {userIdClaim.Value}");
            
		    // Second try: If sub claim is not an integer, try
		    // to look up user by email This handles cases
		    // where the sub claim contains the user's email
		    try
		    {
			using var scope = HttpContext.RequestServices.CreateScope();
			var context = scope.ServiceProvider.GetRequiredService<backend.Data.RqmtMgmtDbContext>();
                
			var user = context.Users
			    .Where(u => u.UserName == userIdClaim.Value || u.Email == userIdClaim.Value)
			    .Select(u => new { u.Id })
			    .FirstOrDefault();
                    
			if (user != null)
			{
			    Console.WriteLine($"[DEBUG] GetCurrentUserId - Found user by sub claim lookup: {user.Id}");
			    return user.Id;
			}
		    }
		    catch (Exception ex)
		    {
			Console.WriteLine($"[DEBUG] GetCurrentUserId - Database lookup error: {ex.Message}");
		    }
		}
	    }
	    else
	    {
		Console.WriteLine("[DEBUG] GetCurrentUserId - No user ID claim found in standard locations");
	    }
    
	    // Third try: Get user ID by looking up the user by email
	    // from the token
	    var emailClaim = User.FindFirst("email") ?? User.FindFirst(System.Security.Claims.ClaimTypes.Email);
	    if (emailClaim != null)
	    {
		Console.WriteLine($"[DEBUG] GetCurrentUserId - Found email claim: {emailClaim.Value}, attempting user lookup");
		try
		{
		    using var scope = HttpContext.RequestServices.CreateScope();
		    var context = scope.ServiceProvider.GetRequiredService<backend.Data.RqmtMgmtDbContext>();
            
		    var user = context.Users
			.Where(u => u.Email == emailClaim.Value)
			.Select(u => new { u.Id })
			.FirstOrDefault();
                
		    if (user != null)
		    {
			Console.WriteLine($"[DEBUG] GetCurrentUserId - Found user by email lookup: {user.Id}");
			return user.Id;
		    }
		}
		catch (Exception ex)
		{
		    Console.WriteLine($"[DEBUG] GetCurrentUserId - Email lookup error: {ex.Message}");
		}
	    }
    
	    // Fourth try: Check for X-User-Id header for
	    // development/testing scenarios
	    if (HttpContext.Items.TryGetValue("UserId", out var impersonatedUserId) && 
		int.TryParse(impersonatedUserId?.ToString(), out var impersonatedId))
	    {
		Console.WriteLine($"[DEBUG] GetCurrentUserId - Using impersonated user ID: {impersonatedId}");
		return impersonatedId;
	    }
    
	    Console.WriteLine("[DEBUG] GetCurrentUserId - No valid user ID found, returning null");
	    return null;
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
        public async Task<ActionResult<PagedResult<TestSuiteDto>>> GetProjectTestSuites(
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
                    SortDescending = sortDescending
                };

                var result = await _testSuiteService.GetPagedByProjectIdAsync(id, parameters);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets test plans for a specific project.
        /// </summary>
        [HttpGet("{id}/test-plans")]
        public async Task<ActionResult<PagedResult<TestPlanDto>>> GetProjectTestPlans(
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
                    SortDescending = sortDescending
                };

                var result = await _testPlanService.GetPagedByProjectIdAsync(id, parameters);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

            /// <summary>
            /// Gets test cases for a specific project.
            /// This includes test cases from all test suites belonging to the project, plus any unassigned test cases.
            /// </summary>
            [HttpGet("{id}/test-cases")]
            public async Task<ActionResult<PagedResult<TestCaseDto>>> GetProjectTestCases(
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
                        SortDescending = sortDescending
                    };

                    var result = await _testCaseService.GetPagedByProjectIdAsync(id, parameters);
                    return Ok(result);
                }
                catch (Exception ex)
                {
                    return StatusCode(500, $"Internal server error: {ex.Message}");
                }
            }

        /// <summary>
        /// Gets test suites for a specific project (alternative route for frontend compatibility).
        /// </summary>
        [HttpGet("{id}/testsuites")]
        public async Task<ActionResult<PagedResult<TestSuiteDto>>> GetProjectTestSuitesAlt(
            int id, 
            [FromQuery] int page = 1, 
            [FromQuery] int pageSize = 20,
            [FromQuery] string? searchTerm = null,
            [FromQuery] string? sortBy = null,
            [FromQuery] bool sortDescending = false)
        {
            // Delegate to the main implementation
            return await GetProjectTestSuites(id, page, pageSize, searchTerm, sortBy, sortDescending);
        }

        /// <summary>
        /// Gets test plans for a specific project (alternative route for frontend compatibility).
        /// </summary>
        [HttpGet("{id}/testplans")]
        public async Task<ActionResult<PagedResult<TestPlanDto>>> GetProjectTestPlansAlt(
            int id, 
            [FromQuery] int page = 1, 
            [FromQuery] int pageSize = 20,
            [FromQuery] string? searchTerm = null,
            [FromQuery] string? sortBy = null,
            [FromQuery] bool sortDescending = false)
        {
            // Delegate to the main implementation
            return await GetProjectTestPlans(id, page, pageSize, searchTerm, sortBy, sortDescending);
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
