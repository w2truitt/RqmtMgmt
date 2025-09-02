using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.Data;
using backend.Models;
using backend.Services;
using Microsoft.EntityFrameworkCore;
using Moq;
using RqmtMgmtShared;
using Xunit;

namespace backend.Tests
{
    public class ProjectServiceTests
    {
        [Fact]
        public async Task GetProjectsAsync_FiltersWorkAsExpected()
        {
            using var db = TestDataHelper.GetDbContext(nameof(GetProjectsAsync_FiltersWorkAsExpected));
            var user = new User { UserName = "user1", Email = "user1@example.com", CreatedAt = DateTime.UtcNow };
            db.Users.Add(user);
            await db.SaveChangesAsync();
            db.Projects.Add(new Project { Name = "SearchMe", Code = "CODE1", Description = "FindMe", Status = ProjectStatus.Active, OwnerId = user.Id, CreatedAt = DateTime.UtcNow });
            db.Projects.Add(new Project { Name = "Other", Code = "CODE2", Description = "Desc", Status = ProjectStatus.OnHold, OwnerId = user.Id, CreatedAt = DateTime.UtcNow });
            await db.SaveChangesAsync();
            var service = new ProjectService(db);
            // SearchTerm
            var filter = new ProjectFilterDto { Page = 1, PageSize = 10, SearchTerm = "SearchMe" };
            var result = await service.GetProjectsAsync(filter);
            Assert.Single(result.Items);
            // Status
            filter = new ProjectFilterDto { Page = 1, PageSize = 10, Status = ProjectStatus.OnHold };
            result = await service.GetProjectsAsync(filter);
            Assert.Single(result.Items);
            // OwnerId
            filter = new ProjectFilterDto { Page = 1, PageSize = 10, OwnerId = user.Id };
            result = await service.GetProjectsAsync(filter);
            Assert.Equal(2, result.Items.Count);
        }

        [Fact]
        public async Task UpdateProjectAsync_NonExistentProject_ReturnsNull()
        {
            using var db = TestDataHelper.GetDbContext(nameof(UpdateProjectAsync_NonExistentProject_ReturnsNull));
            var service = new ProjectService(db);
            var updateDto = new UpdateProjectDto { Name = "X", Code = "X", Status = ProjectStatus.Active, OwnerId = 1 };
            var result = await service.UpdateProjectAsync(9999, updateDto);
            Assert.Null(result);
        }

        [Fact]
        public async Task DeleteProjectAsync_NonExistentProject_ReturnsFalse()
        {
            using var db = TestDataHelper.GetDbContext(nameof(DeleteProjectAsync_NonExistentProject_ReturnsFalse));
            var service = new ProjectService(db);
            var result = await service.DeleteProjectAsync(12345);
            Assert.False(result);
        }

        [Fact]
        public async Task GetProjectTeamMembersAsync_InvalidProjectId_ReturnsEmptyList()
        {
            using var db = TestDataHelper.GetDbContext(nameof(GetProjectTeamMembersAsync_InvalidProjectId_ReturnsEmptyList));
            var service = new ProjectService(db);
            var result = await service.GetProjectTeamMembersAsync(999);
            Assert.Empty(result);
        }

        [Fact]
        public async Task AddTeamMemberAsync_NonExistentUser_ReturnsNull()
        {
            using var db = TestDataHelper.GetDbContext(nameof(AddTeamMemberAsync_NonExistentUser_ReturnsNull));
            var user = new User { UserName = "user1", Email = "user1@example.com", CreatedAt = DateTime.UtcNow };
            db.Users.Add(user); await db.SaveChangesAsync();
            var project = new Project { Name = "NP", Code = "NP", Status = ProjectStatus.Active, OwnerId = user.Id, CreatedAt = DateTime.UtcNow };
            db.Projects.Add(project); await db.SaveChangesAsync();
            var service = new ProjectService(db);
            // UserId 999 does not exist
            var result = await service.AddTeamMemberAsync(project.Id, new AddProjectTeamMemberDto { UserId = 999, Role = ProjectRole.Developer });
            Assert.Null(result);
        }

        [Fact]
        public async Task UpdateTeamMemberAsync_NonExistentMember_ReturnsNull()
        {
            using var db = TestDataHelper.GetDbContext(nameof(UpdateTeamMemberAsync_NonExistentMember_ReturnsNull));
            var service = new ProjectService(db);
            var result = await service.UpdateTeamMemberAsync(1, 2, new UpdateProjectTeamMemberDto { Role = ProjectRole.QAEngineer, IsActive = true });
            Assert.Null(result);
        }

        [Fact]
        public async Task RemoveTeamMemberAsync_NonExistentMember_ReturnsFalse()
        {
            using var db = TestDataHelper.GetDbContext(nameof(RemoveTeamMemberAsync_NonExistentMember_ReturnsFalse));
            var service = new ProjectService(db);
            var result = await service.RemoveTeamMemberAsync(1, 2);
            Assert.False(result);
        }

        [Fact]
        public async Task GetUserProjectsAsync_UserWithNoProjects_ReturnsEmptyList()
        {
            using var db = TestDataHelper.GetDbContext(nameof(GetUserProjectsAsync_UserWithNoProjects_ReturnsEmptyList));
            var service = new ProjectService(db);
            var result = await service.GetUserProjectsAsync(42);
            Assert.Empty(result);
        }

        [Fact]
        public async Task UserHasAccessToProjectAsync_FalseForNonExistent()
        {
            using var db = TestDataHelper.GetDbContext(nameof(UserHasAccessToProjectAsync_FalseForNonExistent));
            var service = new ProjectService(db);
            Assert.False(await service.UserHasAccessToProjectAsync(1, 2));
        }

        [Fact]
        public async Task UserHasRoleInProjectAsync_FalseForNonExistent()
        {
            using var db = TestDataHelper.GetDbContext(nameof(UserHasRoleInProjectAsync_FalseForNonExistent));
            var service = new ProjectService(db);
            Assert.False(await service.UserHasRoleInProjectAsync(1, 2, ProjectRole.QAEngineer));
        }

        [Fact]
        public async Task GenerateNextRequirementIdAsync_ThrowsForMissingProject()
        {
            using var db = TestDataHelper.GetDbContext(nameof(GenerateNextRequirementIdAsync_ThrowsForMissingProject));
            var service = new ProjectService(db);
            await Assert.ThrowsAsync<ArgumentException>(() => service.GenerateNextRequirementIdAsync(999));
        }
        [Fact]
        public async Task GetProjectsAsync_ReturnsPagedProjects()
        {
            using var db = TestDataHelper.GetDbContext(nameof(GetProjectsAsync_ReturnsPagedProjects));
            var user = new User { UserName = "user1", Email = "user1@example.com", CreatedAt = DateTime.UtcNow };
            db.Users.Add(user);
            await db.SaveChangesAsync();
            db.Projects.Add(new Project { Name = "A", Code = "A", Description = "desc", Status = ProjectStatus.Active, OwnerId = user.Id, CreatedAt = DateTime.UtcNow });
            db.Projects.Add(new Project { Name = "B", Code = "B", Description = "desc", Status = ProjectStatus.Active, OwnerId = user.Id, CreatedAt = DateTime.UtcNow });
            await db.SaveChangesAsync();
            var service = new ProjectService(db);
            var filter = new ProjectFilterDto { Page = 1, PageSize = 10 };

            var result = await service.GetProjectsAsync(filter);
            Assert.True(result.Items.Count >= 2);
        }

        [Fact]
        public async Task GetProjectByIdAsync_ReturnsNullIfMissing()
        {
            using var db = TestDataHelper.GetDbContext(nameof(GetProjectByIdAsync_ReturnsNullIfMissing));
            var service = new ProjectService(db);
            var result = await service.GetProjectByIdAsync(999);
            Assert.Null(result);
        }

        [Fact]
        public async Task GetProjectByCodeAsync_ReturnsProject()
        {
            using var db = TestDataHelper.GetDbContext(nameof(GetProjectByCodeAsync_ReturnsProject));
            var user = new User { UserName = "user1", Email = "user1@example.com", CreatedAt = DateTime.UtcNow };
            db.Users.Add(user);
            var project = new Project { Name = "Proj", Code = "P", Status = ProjectStatus.Active, OwnerId = user.Id, CreatedAt = DateTime.UtcNow };
            db.Projects.Add(project);
            await db.SaveChangesAsync();
            var service = new ProjectService(db);
            var result = await service.GetProjectByCodeAsync("P");
            Assert.NotNull(result);
            Assert.Equal("Proj", result.Name);
        }

        [Fact]
        public async Task CreateProjectAsync_CreatesAndReturnsProject()
        {
            using var db = TestDataHelper.GetDbContext(nameof(CreateProjectAsync_CreatesAndReturnsProject));
            var user = new User { UserName = "user1", Email = "user1@example.com", CreatedAt = DateTime.UtcNow };
            db.Users.Add(user);
            await db.SaveChangesAsync();
            var service = new ProjectService(db);
            var dto = new CreateProjectDto { Name = "New", Code = "NEW", Status = ProjectStatus.Active, OwnerId = user.Id };
            var result = await service.CreateProjectAsync(dto);
            Assert.NotNull(result);
            Assert.Equal("New", result.Name);
        }

        [Fact]
        public async Task UpdateProjectAsync_UpdatesProject()
        {
            using var db = TestDataHelper.GetDbContext(nameof(UpdateProjectAsync_UpdatesProject));
            var user = new User { UserName = "user1", Email = "user1@example.com", CreatedAt = DateTime.UtcNow };
            db.Users.Add(user); await db.SaveChangesAsync();
            var project = new Project { Name = "Old", Code = "OLD", Status = ProjectStatus.Active, OwnerId = user.Id, CreatedAt = DateTime.UtcNow };
            db.Projects.Add(project); await db.SaveChangesAsync();
            var service = new ProjectService(db);
            var updateDto = new UpdateProjectDto { Name = "Updated", Code = "UPD", Description = "desc", Status = ProjectStatus.Archived, OwnerId = user.Id };
            var result = await service.UpdateProjectAsync(project.Id, updateDto);
            Assert.NotNull(result);
            Assert.Equal("Updated", result.Name);
            Assert.Equal(ProjectStatus.Archived, result.Status);
        }

        [Fact]
        public async Task DeleteProjectAsync_DeletesProject()
        {
            using var db = TestDataHelper.GetDbContext(nameof(DeleteProjectAsync_DeletesProject));
            var user = new User { UserName = "user1", Email = "user1@example.com", CreatedAt = DateTime.UtcNow };
            db.Users.Add(user); await db.SaveChangesAsync();
            var project = new Project { Name = "ToDelete", Code = "DEL", Status = ProjectStatus.Active, OwnerId = user.Id, CreatedAt = DateTime.UtcNow };
            db.Projects.Add(project); await db.SaveChangesAsync();
            var service = new ProjectService(db);
            var deleted = await service.DeleteProjectAsync(project.Id);
            Assert.True(deleted);
            Assert.Null(await service.GetProjectByIdAsync(project.Id));
        }

        [Fact]
        public async Task GetProjectTeamMembersAsync_ReturnsTeam()
        {
            using var db = TestDataHelper.GetDbContext(nameof(GetProjectTeamMembersAsync_ReturnsTeam));
            var user = new User { UserName = "user1", Email = "user1@example.com", CreatedAt = DateTime.UtcNow };
            db.Users.Add(user); await db.SaveChangesAsync();
            var project = new Project { Name = "Team", Code = "TEAM", Status = ProjectStatus.Active, OwnerId = user.Id, CreatedAt = DateTime.UtcNow };
            db.Projects.Add(project); await db.SaveChangesAsync();
            var teamMember = new ProjectTeamMember { ProjectId = project.Id, UserId = user.Id, Role = ProjectRole.ProjectOwner, JoinedAt = DateTime.UtcNow, IsActive = true };
            db.ProjectTeamMembers.Add(teamMember); await db.SaveChangesAsync();
            var service = new ProjectService(db);
            var result = await service.GetProjectTeamMembersAsync(project.Id);
            Assert.Single(result);
            Assert.Equal(user.Id, result[0].UserId);
        }

        [Fact]
        public async Task AddTeamMemberAsync_AddsOrUpdatesMember()
        {
            using var db = TestDataHelper.GetDbContext(nameof(AddTeamMemberAsync_AddsOrUpdatesMember));
            var user = new User { UserName = "user1", Email = "user1@example.com", CreatedAt = DateTime.UtcNow };
            db.Users.Add(user); await db.SaveChangesAsync();
            var project = new Project { Name = "AT", Code = "AT", Status = ProjectStatus.Active, OwnerId = user.Id, CreatedAt = DateTime.UtcNow };
            db.Projects.Add(project); await db.SaveChangesAsync();
            var service = new ProjectService(db);
            var addDto = new AddProjectTeamMemberDto { UserId = user.Id, Role = ProjectRole.QAEngineer };
            var result = await service.AddTeamMemberAsync(project.Id, addDto);
            Assert.NotNull(result);
            Assert.Equal(ProjectRole.QAEngineer, result.Role);

            // Update role
            var addDto2 = new AddProjectTeamMemberDto { UserId = user.Id, Role = ProjectRole.Developer };
            var result2 = await service.AddTeamMemberAsync(project.Id, addDto2);
            Assert.NotNull(result2);
            Assert.Equal(ProjectRole.Developer, result2.Role);
        }

        [Fact]
        public async Task UpdateTeamMemberAsync_UpdatesMember()
        {
            using var db = TestDataHelper.GetDbContext(nameof(UpdateTeamMemberAsync_UpdatesMember));
            var user = new User { UserName = "user1", Email = "user1@example.com", CreatedAt = DateTime.UtcNow };
            db.Users.Add(user); await db.SaveChangesAsync();
            var project = new Project { Name = "UTM", Code = "UTM", Status = ProjectStatus.Active, OwnerId = user.Id, CreatedAt = DateTime.UtcNow };
            db.Projects.Add(project); await db.SaveChangesAsync();
            var teamMember = new ProjectTeamMember { ProjectId = project.Id, UserId = user.Id, Role = ProjectRole.BusinessAnalyst, JoinedAt = DateTime.UtcNow, IsActive = true };
            db.ProjectTeamMembers.Add(teamMember); await db.SaveChangesAsync();
            var service = new ProjectService(db);
            var updateDto = new UpdateProjectTeamMemberDto { Role = ProjectRole.Stakeholder, IsActive = false };
            var result = await service.UpdateTeamMemberAsync(project.Id, user.Id, updateDto);
            Assert.NotNull(result);
            Assert.Equal(ProjectRole.Stakeholder, result.Role);
            Assert.False(result.IsActive);
        }

        [Fact]
        public async Task RemoveTeamMemberAsync_RemovesMember()
        {
            using var db = TestDataHelper.GetDbContext(nameof(RemoveTeamMemberAsync_RemovesMember));
            var user = new User { UserName = "user1", Email = "user1@example.com", CreatedAt = DateTime.UtcNow };
            db.Users.Add(user); await db.SaveChangesAsync();
            var project = new Project { Name = "RM", Code = "RM", Status = ProjectStatus.Active, OwnerId = user.Id, CreatedAt = DateTime.UtcNow };
            db.Projects.Add(project); await db.SaveChangesAsync();
            var teamMember = new ProjectTeamMember { ProjectId = project.Id, UserId = user.Id, Role = ProjectRole.Developer, JoinedAt = DateTime.UtcNow, IsActive = true };
            db.ProjectTeamMembers.Add(teamMember); await db.SaveChangesAsync();
            var service = new ProjectService(db);
            var removed = await service.RemoveTeamMemberAsync(project.Id, user.Id);
            Assert.True(removed);
            var team = await service.GetProjectTeamMembersAsync(project.Id);
            Assert.Empty(team);
        }

        [Fact]
        public async Task GetUserProjectsAsync_ReturnsProjectsForUser()
        {
            using var db = TestDataHelper.GetDbContext(nameof(GetUserProjectsAsync_ReturnsProjectsForUser));
            var user = new User { UserName = "user1", Email = "user1@example.com", CreatedAt = DateTime.UtcNow };
            db.Users.Add(user); await db.SaveChangesAsync();
            var project = new Project { Name = "UP", Code = "UP", Status = ProjectStatus.Active, OwnerId = user.Id, CreatedAt = DateTime.UtcNow };
            db.Projects.Add(project); await db.SaveChangesAsync();
            db.ProjectTeamMembers.Add(new ProjectTeamMember { ProjectId = project.Id, UserId = user.Id, Role = ProjectRole.QAEngineer, JoinedAt = DateTime.UtcNow, IsActive = true });
            await db.SaveChangesAsync();
            var service = new ProjectService(db);
            var projects = await service.GetUserProjectsAsync(user.Id);
            Assert.Single(projects);
            Assert.Equal("UP", projects[0].Code);
        }

        [Fact]
        public async Task UserHasAccessToProjectAsync_Works()
        {
            using var db = TestDataHelper.GetDbContext(nameof(UserHasAccessToProjectAsync_Works));
            var user = new User { UserName = "user1", Email = "user1@example.com", CreatedAt = DateTime.UtcNow };
            db.Users.Add(user); await db.SaveChangesAsync();
            var project = new Project { Name = "UA", Code = "UA", Status = ProjectStatus.Active, OwnerId = user.Id, CreatedAt = DateTime.UtcNow };
            db.Projects.Add(project); await db.SaveChangesAsync();
            db.ProjectTeamMembers.Add(new ProjectTeamMember { ProjectId = project.Id, UserId = user.Id, Role = ProjectRole.Developer, JoinedAt = DateTime.UtcNow, IsActive = true });
            await db.SaveChangesAsync();
            var service = new ProjectService(db);
            Assert.True(await service.UserHasAccessToProjectAsync(user.Id, project.Id));
            Assert.False(await service.UserHasAccessToProjectAsync(999, project.Id));
        }

        [Fact]
        public async Task UserHasRoleInProjectAsync_Works()
        {
            using var db = TestDataHelper.GetDbContext(nameof(UserHasRoleInProjectAsync_Works));
            var user = new User { UserName = "user1", Email = "user1@example.com", CreatedAt = DateTime.UtcNow };
            db.Users.Add(user); await db.SaveChangesAsync();
            var project = new Project { Name = "UR", Code = "UR", Status = ProjectStatus.Active, OwnerId = user.Id, CreatedAt = DateTime.UtcNow };
            db.Projects.Add(project); await db.SaveChangesAsync();
            db.ProjectTeamMembers.Add(new ProjectTeamMember { ProjectId = project.Id, UserId = user.Id, Role = ProjectRole.ScrumMaster, JoinedAt = DateTime.UtcNow, IsActive = true });
            await db.SaveChangesAsync();
            var service = new ProjectService(db);
            Assert.True(await service.UserHasRoleInProjectAsync(user.Id, project.Id, ProjectRole.ScrumMaster));
            Assert.False(await service.UserHasRoleInProjectAsync(user.Id, project.Id, ProjectRole.QAEngineer));
        }

        [Fact]
        public async Task GenerateNextRequirementIdAsync_ReturnsNextId()
        {
            using var db = TestDataHelper.GetDbContext(nameof(GenerateNextRequirementIdAsync_ReturnsNextId));
            var user = new User { UserName = "user1", Email = "user1@example.com", CreatedAt = DateTime.UtcNow };
            db.Users.Add(user); await db.SaveChangesAsync();
            var project = new Project { Name = "GN", Code = "GN", Status = ProjectStatus.Active, OwnerId = user.Id, CreatedAt = DateTime.UtcNow };
            db.Projects.Add(project); await db.SaveChangesAsync();
            db.Requirements.Add(new Requirement { ProjectId = project.Id, Title = "R1", Type = RequirementType.CRS, Status = RequirementStatus.Draft, CreatedBy = user.Id, CreatedAt = DateTime.UtcNow });
            await db.SaveChangesAsync();
            var service = new ProjectService(db);
            var nextId = await service.GenerateNextRequirementIdAsync(project.Id);
            Assert.StartsWith("GN-REQ-", nextId);
        }
    }
}
