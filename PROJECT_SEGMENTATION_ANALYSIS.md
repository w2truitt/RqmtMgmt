# Project-Based Segmentation Analysis & Recommendations

## Current State Analysis

### Existing Data Model
The current system has a **flat organizational structure** where all records are stored together without project-based segmentation:

- **Requirements** - No project association, only hierarchical parent-child relationships
- **Test Cases** - Only associated with Test Suites, no project context
- **Test Suites** - Standalone entities without project grouping
- **Test Plans** - No project association
- **Users/Roles** - Global system roles, not project-specific

### Identified Limitations
1. **No Project Context** - All requirements appear in a single list regardless of which team/project owns them
2. **No Access Control by Project** - Users see all requirements across all projects
3. **No Project-Specific Reporting** - Cannot filter metrics, dashboards, or reports by project
4. **No Team-Based Workflows** - No way to assign different approval processes per project
5. **Scalability Issues** - As the organization grows, the flat structure becomes unwieldy

---

## Recommended Project Segmentation Strategy

### 1. Project Entity Design

```csharp
public class Project
{
    public int Id { get; set; }
    public required string Name { get; set; }        // e.g., "Mobile Banking App"
    public required string Code { get; set; }        // e.g., "MBA" (for requirement prefixes)
    public string? Description { get; set; }
    public ProjectStatus Status { get; set; }        // Active, Archived, OnHold
    public int OwnerId { get; set; }                 // Product Owner
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation Properties
    public User? Owner { get; set; }
    public ICollection<ProjectTeamMember> TeamMembers { get; set; }
    public ICollection<Requirement> Requirements { get; set; }
    public ICollection<TestSuite> TestSuites { get; set; }
    public ICollection<TestPlan> TestPlans { get; set; }
}

public class ProjectTeamMember
{
    public int ProjectId { get; set; }
    public int UserId { get; set; }
    public ProjectRole Role { get; set; }            // ProjectOwner, Developer, QA, ScrumMaster
    public DateTime JoinedAt { get; set; }
    public bool IsActive { get; set; }
    
    // Navigation Properties
    public Project? Project { get; set; }
    public User? User { get; set; }
}

public enum ProjectStatus { Active, Archived, OnHold, Planning }
public enum ProjectRole { ProjectOwner, Developer, QAEngineer, ScrumMaster, BusinessAnalyst, Stakeholder }
```

### 2. Updated Entity Relationships

#### Requirements with Project Context
```csharp
public class Requirement
{
    // Existing properties...
    public int ProjectId { get; set; }               // NEW: Project association
    public string? ProjectCode { get; set; }         // NEW: For requirement IDs like "MBA-REQ-001"
    
    // Navigation Properties
    public Project? Project { get; set; }            // NEW: Project navigation
    // ... existing navigation properties
}
```

#### Test Entities with Project Context
```csharp
public class TestSuite
{
    // Existing properties...
    public int ProjectId { get; set; }               // NEW: Project association
    
    public Project? Project { get; set; }            // NEW: Project navigation
}

public class TestPlan
{
    // Existing properties...
    public int ProjectId { get; set; }               // NEW: Project association
    
    public Project? Project { get; set; }            // NEW: Project navigation
}
```

---

## Implementation Approach

### Phase 1: Database Schema Migration

#### Step 1: Add Project Tables
```sql
-- Create Project table
CREATE TABLE Projects (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(255) NOT NULL,
    Code NVARCHAR(10) NOT NULL UNIQUE,
    Description NVARCHAR(MAX),
    Status INT NOT NULL DEFAULT 0,
    OwnerId INT NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2,
    FOREIGN KEY (OwnerId) REFERENCES Users(Id)
);

-- Create Project Team Members table
CREATE TABLE ProjectTeamMembers (
    ProjectId INT NOT NULL,
    UserId INT NOT NULL,
    Role INT NOT NULL,
    JoinedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    IsActive BIT NOT NULL DEFAULT 1,
    PRIMARY KEY (ProjectId, UserId),
    FOREIGN KEY (ProjectId) REFERENCES Projects(Id),
    FOREIGN KEY (UserId) REFERENCES Users(Id)
);
```

#### Step 2: Add Project Foreign Keys to Existing Tables
```sql
-- Add ProjectId to Requirements
ALTER TABLE Requirements ADD ProjectId INT;
ALTER TABLE Requirements ADD ProjectCode NVARCHAR(10);

-- Add ProjectId to TestSuites
ALTER TABLE TestSuites ADD ProjectId INT;

-- Add ProjectId to TestPlans  
ALTER TABLE TestPlans ADD ProjectId INT;

-- Add foreign key constraints (after data migration)
-- ALTER TABLE Requirements ADD FOREIGN KEY (ProjectId) REFERENCES Projects(Id);
-- ALTER TABLE TestSuites ADD FOREIGN KEY (ProjectId) REFERENCES Projects(Id);
-- ALTER TABLE TestPlans ADD FOREIGN KEY (ProjectId) REFERENCES Projects(Id);
```

#### Step 3: Data Migration Strategy
```csharp
// Migration script to handle existing data
public class AddProjectSegmentationMigration
{
    public void MigrateExistingData()
    {
        // 1. Create a "Default Project" for existing records
        var defaultProject = new Project
        {
            Name = "Legacy Requirements",
            Code = "LEG",
            Status = ProjectStatus.Active,
            OwnerId = GetSystemAdminUserId(),
            CreatedAt = DateTime.UtcNow
        };
        
        // 2. Assign all existing requirements to default project
        // 3. Assign all existing test suites to default project
        // 4. Update requirement codes with project prefix
    }
}
```

### Phase 2: Application Layer Updates

#### Updated DTOs
```csharp
public class ProjectDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Code { get; set; }
    public string? Description { get; set; }
    public ProjectStatus Status { get; set; }
    public int OwnerId { get; set; }
    public string OwnerName { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<ProjectTeamMemberDto> TeamMembers { get; set; }
    public int RequirementCount { get; set; }
    public int TestSuiteCount { get; set; }
}

public class RequirementDto
{
    // Existing properties...
    public int ProjectId { get; set; }               // NEW
    public string ProjectName { get; set; }          // NEW
    public string ProjectCode { get; set; }          // NEW
    public string FullRequirementId { get; set; }    // NEW: "MBA-REQ-001"
}
```

#### Updated Controllers
```csharp
[ApiController]
[Route("api/[controller]")]
public class ProjectsController : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<ProjectDto>>> GetProjects([FromQuery] ProjectFilterDto filter)
    {
        // Return projects user has access to
    }
    
    [HttpGet("{projectId}/requirements")]
    public async Task<ActionResult<PagedResult<RequirementDto>>> GetProjectRequirements(int projectId, [FromQuery] RequirementFilterDto filter)
    {
        // Return requirements for specific project
    }
    
    [HttpGet("{projectId}/team")]
    public async Task<ActionResult<List<ProjectTeamMemberDto>>> GetProjectTeam(int projectId)
    {
        // Return project team members
    }
}

[ApiController]
[Route("api/[controller]")]
public class RequirementsController : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<RequirementDto>>> GetRequirements([FromQuery] RequirementFilterDto filter)
    {
        // Filter by user's accessible projects
        var userProjects = await GetUserAccessibleProjects();
        filter.ProjectIds = userProjects.Select(p => p.Id).ToList();
        
        return await _requirementService.GetRequirementsAsync(filter);
    }
}
```

### Phase 3: Frontend Updates

#### Project Selection Component
```csharp
@page "/projects"
@inject IProjectService ProjectService

<h3>Projects</h3>

<div class="project-grid">
    @foreach (var project in projects)
    {
        <div class="project-card" @onclick="() => NavigateToProject(project.Id)">
            <h4>@project.Name (@project.Code)</h4>
            <p>@project.Description</p>
            <div class="project-stats">
                <span>@project.RequirementCount Requirements</span>
                <span>@project.TestSuiteCount Test Suites</span>
            </div>
            <div class="project-team">
                Owner: @project.OwnerName
            </div>
        </div>
    }
</div>
```

#### Updated Requirements Page
```csharp
@page "/projects/{ProjectId:int}/requirements"
@inject IRequirementService RequirementService

<div class="page-header">
    <h3>Requirements - @projectName</h3>
    <ProjectBreadcrumb ProjectId="ProjectId" />
</div>

<RequirementsList ProjectId="ProjectId" />
```

#### Navigation Updates
```csharp
// Updated navigation structure
Projects/
├── Project Dashboard
├── Requirements
├── Test Suites  
├── Test Plans
├── Reports
└── Team Management

// Breadcrumb navigation
Home > Projects > Mobile Banking App > Requirements > User Authentication
```

---

## User Experience Improvements

### 1. Project-Centric Navigation
- **Project Dashboard** - Landing page showing project overview, recent activity, team members
- **Project Selector** - Quick switcher in header to change active project
- **Breadcrumb Navigation** - Clear hierarchy: Project > Section > Item

### 2. Enhanced Requirement IDs
- **Structured IDs** - `{ProjectCode}-REQ-{Number}` (e.g., "MBA-REQ-001")
- **Automatic Generation** - System generates sequential IDs per project
- **Cross-Project References** - Support linking requirements across projects

### 3. Project-Specific Dashboards
- **Requirements Progress** - By project status and type
- **Test Coverage** - Project-specific test metrics
- **Team Velocity** - Project-based performance tracking
- **Risk Assessment** - Project-specific risk indicators

### 4. Access Control by Project
```csharp
public class ProjectPermissionService
{
    public async Task<bool> CanUserAccessProject(int userId, int projectId)
    {
        // Check if user is team member or has admin role
    }
    
    public async Task<List<Project>> GetUserAccessibleProjects(int userId)
    {
        // Return projects user can access
    }
    
    public async Task<bool> CanUserPerformAction(int userId, int projectId, string action)
    {
        // Check specific permissions (create, edit, delete, approve)
    }
}
```

---

## Migration Strategy for Existing Data

### Option 1: Single Default Project (Recommended for MVP)
1. Create one "Default Project" or "Legacy Project"
2. Assign all existing requirements/test cases to this project
3. Allow users to create new projects going forward
4. Provide tools to move requirements between projects later

### Option 2: Smart Project Detection
1. Analyze existing requirement titles/descriptions for project keywords
2. Create projects based on detected patterns
3. Auto-assign requirements to detected projects
4. Manual review and correction process

### Option 3: Manual Migration
1. Create empty project structure
2. Provide migration tools for admins to:
   - Create projects
   - Bulk move requirements between projects
   - Assign team members to projects

---

## Implementation Timeline

### Sprint 1-2: Database & Backend (2-3 weeks)
- [ ] Create Project entity and relationships
- [ ] Database migration scripts
- [ ] Updated backend services and controllers
- [ ] Project-based access control
- [ ] API endpoints for project management

### Sprint 3-4: Frontend Core (2-3 weeks)  
- [ ] Project selection/dashboard pages
- [ ] Updated navigation with project context
- [ ] Project-filtered requirement lists
- [ ] Project management UI (create/edit projects)

### Sprint 5-6: Advanced Features (2-3 weeks)
- [ ] Project-specific reporting
- [ ] Team management interfaces
- [ ] Cross-project requirement linking
- [ ] Advanced filtering and search

### Sprint 7: Migration & Testing (1-2 weeks)
- [ ] Data migration tools
- [ ] Comprehensive testing
- [ ] User acceptance testing
- [ ] Documentation and training

---

## Benefits of Project Segmentation

### For Product Owners
- **Clear Project Ownership** - Dedicated project spaces with team assignment
- **Project-Specific Reporting** - Metrics and progress tracking per project
- **Better Stakeholder Communication** - Project-focused views and updates

### For Development Teams
- **Focused Work Context** - Only see requirements relevant to their projects
- **Team-Based Collaboration** - Project-specific discussions and decisions
- **Clearer Responsibility** - Project-based role assignments

### For QA Engineers
- **Project-Specific Test Planning** - Test suites organized by project
- **Targeted Test Execution** - Focus testing efforts on specific projects
- **Project Quality Metrics** - Quality tracking per project

### For Organizations
- **Scalability** - System grows with organization without becoming unwieldy
- **Multi-Project Support** - Handle multiple concurrent projects effectively
- **Resource Management** - Track team allocation across projects
- **Audit Trail** - Project-specific compliance and reporting

---

## Recommended Next Steps

1. **Stakeholder Review** - Present this analysis to key stakeholders for feedback
2. **Technical Planning** - Detailed technical design and effort estimation
3. **Pilot Project** - Start with one project to validate the approach
4. **Phased Rollout** - Gradual migration to minimize disruption
5. **Training Plan** - User education on new project-centric workflows

This project segmentation will transform the system from a flat requirements list into a scalable, multi-project enterprise platform that better serves the needs of different teams and stakeholders.