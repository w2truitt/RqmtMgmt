using System;
using System.Collections.Generic;

namespace RqmtMgmtShared
{
    /// <summary>
    /// Data transfer object for Project information.
    /// </summary>
    public class ProjectDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string? Description { get; set; }
        public ProjectStatus Status { get; set; }
        public int OwnerId { get; set; }
        public string OwnerName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<ProjectTeamMemberDto> TeamMembers { get; set; } = new();
        public int RequirementCount { get; set; }
        public int TestSuiteCount { get; set; }
        public int TestPlanCount { get; set; }
    }

    /// <summary>
    /// Data transfer object for Project Team Member information.
    /// </summary>
    public class ProjectTeamMemberDto
    {
        public int ProjectId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public ProjectRole Role { get; set; }
        public DateTime JoinedAt { get; set; }
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// Data transfer object for creating a new project.
    /// </summary>
    public class CreateProjectDto
    {
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string? Description { get; set; }
        public ProjectStatus Status { get; set; } = ProjectStatus.Planning;
        public int OwnerId { get; set; }
    }

    /// <summary>
    /// Data transfer object for updating an existing project.
    /// </summary>
    public class UpdateProjectDto
    {
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string? Description { get; set; }
        public ProjectStatus Status { get; set; }
        public int OwnerId { get; set; }
    }

    /// <summary>
    /// Data transfer object for filtering projects.
    /// </summary>
    public class ProjectFilterDto
    {
        public string? SearchTerm { get; set; }
        public ProjectStatus? Status { get; set; }
        public int? OwnerId { get; set; }
        public bool? UserIsMember { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    /// <summary>
    /// Data transfer object for adding a team member to a project.
    /// </summary>
    public class AddProjectTeamMemberDto
    {
        public int UserId { get; set; }
        public ProjectRole Role { get; set; }
    }

    /// <summary>
    /// Data transfer object for updating a team member's role.
    /// </summary>
    public class UpdateProjectTeamMemberDto
    {
        public ProjectRole Role { get; set; }
        public bool IsActive { get; set; }
    }
}