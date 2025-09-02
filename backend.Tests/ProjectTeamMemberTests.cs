using System;
using backend.Models;
using RqmtMgmtShared;
using Xunit;

namespace backend.Tests
{
    public class ProjectTeamMemberTests
    {
        [Fact]
        public void CanInstantiateProjectTeamMember_WithAllProperties()
        {
            var date = DateTime.UtcNow;
            var teamMember = new ProjectTeamMember
            {
                ProjectId = 1,
                UserId = 2,
                Role = ProjectRole.Developer,
                JoinedAt = date,
                IsActive = true
            };
            Assert.Equal(1, teamMember.ProjectId);
            Assert.Equal(2, teamMember.UserId);
            Assert.Equal(ProjectRole.Developer, teamMember.Role);
            Assert.Equal(date, teamMember.JoinedAt);
            Assert.True(teamMember.IsActive);
        }

        [Fact]
        public void CanSetAndGetUserAndProjectNavigation()
        {
            var user = new User { Id = 3, UserName = "tester", Email = "tester@example.com" };
            var project = new Project { Id = 5, Name = "Proj", Code = "P", Status = ProjectStatus.Active, OwnerId = 3, CreatedAt = DateTime.UtcNow };
            var teamMember = new ProjectTeamMember { User = user, Project = project };
            Assert.Equal(user, teamMember.User);
            Assert.Equal(project, teamMember.Project);
        }
    }
}
