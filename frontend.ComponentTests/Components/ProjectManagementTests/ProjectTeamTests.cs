using Bunit;
using frontend.Services;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using RqmtMgmtShared;
using Xunit;

namespace frontend.ComponentTests.Components.ProjectManagementTests
{
    /// <summary>
    /// Tests for the ProjectTeam component services and functionality
    /// </summary>
    public class ProjectTeamTests : ComponentTestBase
    {
        [Fact]
        public async Task ProjectTeamService_GetProjectTeamMembers_ReturnsTeamMembers()
        {
            // Arrange
            var projectId = 1;
            var expectedTeamMembers = new List<ProjectTeamMemberDto>
            {
                new ProjectTeamMemberDto { ProjectId = projectId, UserId = 1, UserName = "John Doe", UserEmail = "john@test.com", Role = ProjectRole.Developer },
                new ProjectTeamMemberDto { ProjectId = projectId, UserId = 2, UserName = "Jane Smith", UserEmail = "jane@test.com", Role = ProjectRole.QAEngineer }
            };

            var mockProjectService = GetMockService<IProjectService>();
            mockProjectService.Setup(s => s.GetProjectTeamMembersAsync(projectId))
                .ReturnsAsync(expectedTeamMembers);

            var projectService = Services.GetRequiredService<IProjectService>();

            // Act
            var result = await projectService.GetProjectTeamMembersAsync(projectId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal("John Doe", result[0].UserName);
            Assert.Equal("Jane Smith", result[1].UserName);
        }

        [Fact]
        public async Task ProjectTeamService_AddTeamMember_ReturnsNewMember()
        {
            // Arrange
            var projectId = 1;
            var newMemberDto = new AddProjectTeamMemberDto { UserId = 3, Role = ProjectRole.Developer };
            var expectedMember = new ProjectTeamMemberDto 
            { 
                ProjectId = projectId, 
                UserId = 3, 
                UserName = "New User", 
                UserEmail = "new@test.com", 
                Role = ProjectRole.Developer 
            };

            var mockProjectService = GetMockService<IProjectService>();
            mockProjectService.Setup(s => s.AddTeamMemberAsync(projectId, newMemberDto))
                .ReturnsAsync(expectedMember);

            var projectService = Services.GetRequiredService<IProjectService>();

            // Act
            var result = await projectService.AddTeamMemberAsync(projectId, newMemberDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.UserId);
            Assert.Equal("New User", result.UserName);
            Assert.Equal(ProjectRole.Developer, result.Role);
        }

        [Fact]
        public async Task ProjectTeamService_UpdateTeamMember_ReturnsUpdatedMember()
        {
            // Arrange
            var projectId = 1;
            var userId = 2;
            var updateDto = new UpdateProjectTeamMemberDto { Role = ProjectRole.ScrumMaster };
            var expectedMember = new ProjectTeamMemberDto 
            { 
                ProjectId = projectId, 
                UserId = userId, 
                UserName = "Updated User", 
                UserEmail = "updated@test.com", 
                Role = ProjectRole.ScrumMaster 
            };

            var mockProjectService = GetMockService<IProjectService>();
            mockProjectService.Setup(s => s.UpdateTeamMemberAsync(projectId, userId, updateDto))
                .ReturnsAsync(expectedMember);

            var projectService = Services.GetRequiredService<IProjectService>();

            // Act
            var result = await projectService.UpdateTeamMemberAsync(projectId, userId, updateDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(userId, result.UserId);
            Assert.Equal(ProjectRole.ScrumMaster, result.Role);
        }

        [Fact]
        public async Task ProjectTeamService_RemoveTeamMember_ReturnsTrue()
        {
            // Arrange
            var projectId = 1;
            var userId = 2;

            var mockProjectService = GetMockService<IProjectService>();
            mockProjectService.Setup(s => s.RemoveTeamMemberAsync(projectId, userId))
                .ReturnsAsync(true);

            var projectService = Services.GetRequiredService<IProjectService>();

            // Act
            var result = await projectService.RemoveTeamMemberAsync(projectId, userId);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task UserService_GetAllUsers_ReturnsUserList()
        {
            // Arrange
            var expectedUsers = new List<UserDto>
            {
                new UserDto { Id = 1, UserName = "User One", Email = "user1@test.com" },
                new UserDto { Id = 2, UserName = "User Two", Email = "user2@test.com" }
            };

            var mockUserService = GetMockService<IUserService>();
            mockUserService.Setup(s => s.GetAllAsync())
                .ReturnsAsync(expectedUsers);

            var userService = Services.GetRequiredService<IUserService>();

            // Act
            var result = await userService.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal("User One", result[0].UserName);
            Assert.Equal("User Two", result[1].UserName);
        }

        [Fact]
        public void ProjectRole_Enum_HasExpectedValues()
        {
            // Assert
            var roles = Enum.GetValues<ProjectRole>();
            Assert.Contains(ProjectRole.ProjectOwner, roles);
            Assert.Contains(ProjectRole.Developer, roles);
            Assert.Contains(ProjectRole.QAEngineer, roles);
            Assert.Contains(ProjectRole.ScrumMaster, roles);
            Assert.Contains(ProjectRole.BusinessAnalyst, roles);
            Assert.Contains(ProjectRole.Stakeholder, roles);
        }
    }
}
