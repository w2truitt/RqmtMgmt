using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Models;
using RqmtMgmtShared;

namespace backend.Services
{
    /// <summary>
    /// Service implementation for managing projects and project team members.
    /// PERFORMANCE OPTIMIZED: Added AsNoTracking() and optimized includes for better performance.
    /// </summary>
    public class ProjectService : IProjectService
    {
        private readonly RqmtMgmtDbContext _context;

        public ProjectService(RqmtMgmtDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<ProjectDto>> GetProjectsAsync(ProjectFilterDto filter)
        {
            // PERFORMANCE OPTIMIZATION: For list views, only include essential data and use AsNoTracking
            var query = _context.Projects
                .Include(p => p.Owner)
                .AsNoTracking() // Critical: Don't track entities for read-only operations
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                query = query.Where(p => p.Name.Contains(filter.SearchTerm) || 
                                       p.Code.Contains(filter.SearchTerm) ||
                                       (p.Description != null && p.Description.Contains(filter.SearchTerm)));
            }

            if (filter.Status.HasValue)
            {
                query = query.Where(p => p.Status == filter.Status.Value);
            }

            if (filter.OwnerId.HasValue)
            {
                query = query.Where(p => p.OwnerId == filter.OwnerId.Value);
            }

            if (filter.UserIsMember.HasValue && filter.UserIsMember.Value)
            {
                // This would need the current user ID to be passed in the filter
                // For now, we'll skip this filter
            }

            var totalCount = await query.CountAsync();

            // PERFORMANCE OPTIMIZATION: Use projection to load only necessary data for list view
            var projects = await query
                .OrderBy(p => p.Name)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(p => new ProjectDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Code = p.Code,
                    Description = p.Description,
                    Status = p.Status,
                    OwnerId = p.OwnerId,
                    OwnerName = p.Owner != null ? p.Owner.UserName : "Unknown",
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt,
                    // PERFORMANCE OPTIMIZED: Use efficient database COUNT queries with new composite indexes
                    RequirementCount = _context.Requirements.Where(r => r.ProjectId == p.Id).Count(),
                    TestSuiteCount = _context.TestSuites.Where(ts => ts.ProjectId == p.Id).Count(),
                    TestPlanCount = _context.TestPlans.Where(tp => tp.ProjectId == p.Id).Count(),
                    TeamMembers = new List<ProjectTeamMemberDto>() // Empty for list view - load separately if needed
                })
                .ToListAsync();

            return new PagedResult<ProjectDto>
            {
                Items = projects,
                TotalItems = totalCount,
                PageNumber = filter.Page,
                PageSize = filter.PageSize
            };
        }

        public async Task<ProjectDto?> GetProjectByIdAsync(int projectId)
        {
                // PERFORMANCE OPTIMIZATION: Load only essential project data, then counts separately
            var project = await _context.Projects
                .Include(p => p.Owner)
                    .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == projectId);

                if (project == null) return null;

                // Load counts with efficient separate queries instead of expensive includes
                var requirementCount = await _context.Requirements
                    .AsNoTracking()
                    .CountAsync(r => r.ProjectId == projectId);

                var testSuiteCount = await _context.TestSuites
                    .AsNoTracking()
                    .CountAsync(ts => ts.ProjectId == projectId);

                var testPlanCount = await _context.TestPlans
                    .AsNoTracking()
                    .CountAsync(tp => tp.ProjectId == projectId);

                // Load team members separately (only essential data)
                var teamMembers = await _context.ProjectTeamMembers
                    .Include(tm => tm.User)
                    .AsNoTracking()
                    .Where(tm => tm.ProjectId == projectId)
                    .ToListAsync();

                return new ProjectDto
                {
                    Id = project.Id,
                    Name = project.Name,
                    Code = project.Code,
                    Description = project.Description,
                    Status = project.Status,
                    OwnerId = project.OwnerId,
                    OwnerName = project.Owner?.UserName ?? "Unknown",
                    CreatedAt = project.CreatedAt,
                    UpdatedAt = project.UpdatedAt,
                    TeamMembers = teamMembers.Select(MapTeamMemberToDto).ToList(),
                    RequirementCount = requirementCount,
                    TestSuiteCount = testSuiteCount,
                    TestPlanCount = testPlanCount
                };
        }

        public async Task<ProjectDto?> GetProjectByCodeAsync(string code)
        {
                // PERFORMANCE OPTIMIZATION: Load only essential project data, then counts separately
            var project = await _context.Projects
                .Include(p => p.Owner)
                    .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Code == code);

                if (project == null) return null;

                // Load counts with efficient separate queries instead of expensive includes
                var requirementCount = await _context.Requirements
                    .AsNoTracking()
                    .CountAsync(r => r.ProjectId == project.Id);

                var testSuiteCount = await _context.TestSuites
                    .AsNoTracking()
                    .CountAsync(ts => ts.ProjectId == project.Id);

                var testPlanCount = await _context.TestPlans
                    .AsNoTracking()
                    .CountAsync(tp => tp.ProjectId == project.Id);

                // Load team members separately (only essential data)
                var teamMembers = await _context.ProjectTeamMembers
                    .Include(tm => tm.User)
                    .AsNoTracking()
                    .Where(tm => tm.ProjectId == project.Id)
                    .ToListAsync();

                return new ProjectDto
                {
                    Id = project.Id,
                    Name = project.Name,
                    Code = project.Code,
                    Description = project.Description,
                    Status = project.Status,
                    OwnerId = project.OwnerId,
                    OwnerName = project.Owner?.UserName ?? "Unknown",
                    CreatedAt = project.CreatedAt,
                    UpdatedAt = project.UpdatedAt,
                    TeamMembers = teamMembers.Select(MapTeamMemberToDto).ToList(),
                    RequirementCount = requirementCount,
                    TestSuiteCount = testSuiteCount,
                    TestPlanCount = testPlanCount
                };
        }

        public async Task<ProjectDto> CreateProjectAsync(CreateProjectDto createProjectDto)
        {
            var project = new Project
            {
                Name = createProjectDto.Name,
                Code = createProjectDto.Code,
                Description = createProjectDto.Description,
                Status = createProjectDto.Status,
                OwnerId = createProjectDto.OwnerId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Projects.Add(project);
            await _context.SaveChangesAsync();

            // Add the owner as a team member with ProjectOwner role
            var teamMember = new ProjectTeamMember
            {
                ProjectId = project.Id,
                UserId = project.OwnerId,
                Role = ProjectRole.ProjectOwner,
                JoinedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.ProjectTeamMembers.Add(teamMember);
            await _context.SaveChangesAsync();

            return await GetProjectByIdAsync(project.Id) ?? throw new InvalidOperationException("Failed to retrieve created project");
        }

        public async Task<ProjectDto?> UpdateProjectAsync(int projectId, UpdateProjectDto updateProjectDto)
        {
            var project = await _context.Projects.FindAsync(projectId);
            if (project == null) return null;

            project.Name = updateProjectDto.Name;
            project.Code = updateProjectDto.Code;
            project.Description = updateProjectDto.Description;
            project.Status = updateProjectDto.Status;
            project.OwnerId = updateProjectDto.OwnerId;
            project.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return await GetProjectByIdAsync(projectId);
        }

        public async Task<bool> DeleteProjectAsync(int projectId)
        {
            var project = await _context.Projects.FindAsync(projectId);
            if (project == null) return false;

            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<List<ProjectTeamMemberDto>> GetProjectTeamMembersAsync(int projectId)
        {
            // PERFORMANCE OPTIMIZATION: Add AsNoTracking for read-only operations
            var teamMembers = await _context.ProjectTeamMembers
                .Include(tm => tm.User)
                .AsNoTracking()
                .Where(tm => tm.ProjectId == projectId)
                .ToListAsync();

            return teamMembers.Select(MapTeamMemberToDto).ToList();
        }

        public async Task<ProjectTeamMemberDto?> AddTeamMemberAsync(int projectId, AddProjectTeamMemberDto addTeamMemberDto)
        {
            // First, verify that the project exists
            var project = await _context.Projects.FindAsync(projectId);
            if (project == null)
            {
                return null; // Project not found
            }

            // Verify that the user exists
            var user = await _context.Users.FindAsync(addTeamMemberDto.UserId);
            if (user == null)
            {
                return null; // User not found
            }

            // Check if the team member already exists
            var existingMember = await _context.ProjectTeamMembers
                .FirstOrDefaultAsync(tm => tm.ProjectId == projectId && tm.UserId == addTeamMemberDto.UserId);

            if (existingMember != null)
            {
                // Update existing member
                existingMember.Role = addTeamMemberDto.Role;
                existingMember.IsActive = true;
            }
            else
            {
                // Add new team member
                var teamMember = new ProjectTeamMember
                {
                    ProjectId = projectId,
                    UserId = addTeamMemberDto.UserId,
                    Role = addTeamMemberDto.Role,
                    JoinedAt = DateTime.UtcNow,
                    IsActive = true
                };

                _context.ProjectTeamMembers.Add(teamMember);
            }

            await _context.SaveChangesAsync();

            var updatedMember = await _context.ProjectTeamMembers
                .Include(tm => tm.User)
                .AsNoTracking() // PERFORMANCE OPTIMIZATION: Add AsNoTracking
                .FirstOrDefaultAsync(tm => tm.ProjectId == projectId && tm.UserId == addTeamMemberDto.UserId);

            return updatedMember != null ? MapTeamMemberToDto(updatedMember) : null;
        }

        public async Task<ProjectTeamMemberDto?> UpdateTeamMemberAsync(int projectId, int userId, UpdateProjectTeamMemberDto updateTeamMemberDto)
        {
            var teamMember = await _context.ProjectTeamMembers
                .Include(tm => tm.User)
                .FirstOrDefaultAsync(tm => tm.ProjectId == projectId && tm.UserId == userId);

            if (teamMember == null) return null;

            teamMember.Role = updateTeamMemberDto.Role;
            teamMember.IsActive = updateTeamMemberDto.IsActive;

            await _context.SaveChangesAsync();

            return MapTeamMemberToDto(teamMember);
        }

        public async Task<bool> RemoveTeamMemberAsync(int projectId, int userId)
        {
            var teamMember = await _context.ProjectTeamMembers
                .FirstOrDefaultAsync(tm => tm.ProjectId == projectId && tm.UserId == userId);

            if (teamMember == null) return false;

            _context.ProjectTeamMembers.Remove(teamMember);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<List<ProjectDto>> GetUserProjectsAsync(int userId)
        {
            // PERFORMANCE OPTIMIZATION: Add AsNoTracking and optimize includes
            var projects = await _context.ProjectTeamMembers
                .Include(tm => tm.Project)
                .ThenInclude(p => p!.Owner)
                .AsNoTracking()
                .Where(tm => tm.UserId == userId && tm.IsActive)
                .Select(tm => tm.Project!)
                .Distinct()
                .ToListAsync();

            return projects.Select(MapToDto).ToList();
        }

        public async Task<bool> UserHasAccessToProjectAsync(int userId, int projectId)
        {
            return await _context.ProjectTeamMembers
                .AsNoTracking() // PERFORMANCE OPTIMIZATION: Add AsNoTracking
                .AnyAsync(tm => tm.ProjectId == projectId && tm.UserId == userId && tm.IsActive);
        }

        public async Task<bool> UserHasRoleInProjectAsync(int userId, int projectId, ProjectRole role)
        {
            return await _context.ProjectTeamMembers
                .AsNoTracking() // PERFORMANCE OPTIMIZATION: Add AsNoTracking
                .AnyAsync(tm => tm.ProjectId == projectId && tm.UserId == userId && tm.Role == role && tm.IsActive);
        }

        public async Task<string> GenerateNextRequirementIdAsync(int projectId)
        {
            var project = await _context.Projects.FindAsync(projectId);
            if (project == null) throw new ArgumentException("Project not found", nameof(projectId));

            var maxRequirementNumber = await _context.Requirements
                .AsNoTracking() // PERFORMANCE OPTIMIZATION: Add AsNoTracking
                .Where(r => r.ProjectId == projectId)
                .CountAsync() + 1;

            return $"{project.Code}-REQ-{maxRequirementNumber:D3}";
        }

        private static ProjectDto MapToDto(Project project)
        {
            return new ProjectDto
            {
                Id = project.Id,
                Name = project.Name,
                Code = project.Code,
                Description = project.Description,
                Status = project.Status,
                OwnerId = project.OwnerId,
                OwnerName = project.Owner?.UserName ?? "Unknown",
                CreatedAt = project.CreatedAt,
                UpdatedAt = project.UpdatedAt,
                TeamMembers = project.TeamMembers?.Select(MapTeamMemberToDto).ToList() ?? new List<ProjectTeamMemberDto>(),
                RequirementCount = project.Requirements?.Count ?? 0,
                TestSuiteCount = project.TestSuites?.Count ?? 0,
                TestPlanCount = project.TestPlans?.Count ?? 0
            };
        }

        private static ProjectTeamMemberDto MapTeamMemberToDto(ProjectTeamMember teamMember)
        {
            return new ProjectTeamMemberDto
            {
                ProjectId = teamMember.ProjectId,
                UserId = teamMember.UserId,
                UserName = teamMember.User?.UserName ?? "Unknown",
                UserEmail = teamMember.User?.Email ?? "Unknown",
                Role = teamMember.Role,
                JoinedAt = teamMember.JoinedAt,
                IsActive = teamMember.IsActive
            };
        }
    }
}