using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using backend.Controllers;
using backend.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;
using RqmtMgmtShared;
using Xunit;

namespace backend.Tests
{
    public class ProjectsControllerTests
    {
        private ProjectsController CreateController(Mock<IProjectService> projectService = null, Mock<IRequirementService> requirementService = null)
        {
            var mockProjectService = projectService ?? new Mock<IProjectService>();
            var mockRequirementService = requirementService ?? new Mock<IRequirementService>();
            return new ProjectsController(mockProjectService.Object, mockRequirementService.Object);
        }

        [Fact]
        public async Task CreateProject_InvalidModelState_ReturnsBadRequest()
        {
            var mock = new Mock<IProjectService>();
            var controller = CreateController(mock);
            controller.ModelState.AddModelError("Name", "Required");
            var dto = new CreateProjectDto();
            var result = await controller.CreateProject(dto);
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task CreateProject_ServiceThrowsException_Returns500()
        {
            var mock = new Mock<IProjectService>();
            mock.Setup(s => s.CreateProjectAsync(It.IsAny<CreateProjectDto>())).ThrowsAsync(new Exception("fail"));
            var controller = CreateController(mock);
            var dto = new CreateProjectDto { Name = "fail", Code = "fail", Status = ProjectStatus.Active, OwnerId = 1 };
            var result = await controller.CreateProject(dto);
            var obj = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, obj.StatusCode);
        }

        [Fact]
        public async Task UpdateProject_InvalidModelState_ReturnsBadRequest()
        {
            var mock = new Mock<IProjectService>();
            var controller = CreateController(mock);
            controller.ModelState.AddModelError("desc", "error");
            var dto = new UpdateProjectDto { Name = "X", Code = "X", Status = ProjectStatus.Active, OwnerId = 1 };
            var result = await controller.UpdateProject(1, dto);
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task UpdateProject_ServiceThrowsException_Returns500()
        {
            var mock = new Mock<IProjectService>();
            mock.Setup(s => s.UpdateProjectAsync(It.IsAny<int>(), It.IsAny<UpdateProjectDto>())).ThrowsAsync(new Exception("fail"));
            var controller = CreateController(mock);
            var dto = new UpdateProjectDto { Name = "X", Code = "X", Status = ProjectStatus.Active, OwnerId = 1 };
            var result = await controller.UpdateProject(1, dto);
            var obj = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, obj.StatusCode);
        }

        [Fact]
        public async Task DeleteProject_ServiceThrowsException_Returns500()
        {
            var mock = new Mock<IProjectService>();
            mock.Setup(s => s.DeleteProjectAsync(It.IsAny<int>())).ThrowsAsync(new Exception("fail"));
            var controller = CreateController(mock);
            var result = await controller.DeleteProject(1);
            var obj = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, obj.StatusCode);
        }

        [Fact]
        public async Task GetProjectTeamMembers_ServiceThrows_Returns500()
        {
            var mock = new Mock<IProjectService>();
            // Mock project exists to bypass the NotFound check
            mock.Setup(s => s.GetProjectByIdAsync(It.IsAny<int>())).ReturnsAsync(new ProjectDto { Id = 1 });
            mock.Setup(s => s.GetProjectTeamMembersAsync(It.IsAny<int>())).ThrowsAsync(new Exception("fail"));
            var controller = CreateController(mock);
            var result = await controller.GetProjectTeamMembers(1);
            var obj = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, obj.StatusCode);
        }

        [Fact]
        public async Task AddTeamMember_InvalidModelState_ReturnsBadRequest()
        {
            var mock = new Mock<IProjectService>();
            var controller = CreateController(mock);
            controller.ModelState.AddModelError("role", "error");
            var dto = new AddProjectTeamMemberDto { UserId = 1, Role = ProjectRole.QAEngineer };
            var result = await controller.AddTeamMember(1, dto);
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task AddTeamMember_ServiceThrows_Returns500()
        {
            var mock = new Mock<IProjectService>();
            mock.Setup(s => s.AddTeamMemberAsync(It.IsAny<int>(), It.IsAny<AddProjectTeamMemberDto>())).ThrowsAsync(new Exception("fail"));
            var controller = CreateController(mock);
            var dto = new AddProjectTeamMemberDto { UserId = 1, Role = ProjectRole.QAEngineer };
            var result = await controller.AddTeamMember(1, dto);
            var obj = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, obj.StatusCode);
        }

        [Fact]
        public async Task UpdateTeamMember_InvalidModelState_ReturnsBadRequest()
        {
            var mock = new Mock<IProjectService>();
            var controller = CreateController(mock);
            controller.ModelState.AddModelError("role", "error");
            var dto = new UpdateProjectTeamMemberDto { Role = ProjectRole.QAEngineer, IsActive = true };
            var result = await controller.UpdateTeamMember(1, 2, dto);
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task UpdateTeamMember_ServiceThrows_Returns500()
        {
            var mock = new Mock<IProjectService>();
            mock.Setup(s => s.UpdateTeamMemberAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<UpdateProjectTeamMemberDto>())).ThrowsAsync(new Exception("fail"));
            var controller = CreateController(mock);
            var dto = new UpdateProjectTeamMemberDto { Role = ProjectRole.QAEngineer, IsActive = true };
            var result = await controller.UpdateTeamMember(1, 2, dto);
            var obj = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, obj.StatusCode);
        }

        [Fact]
        public async Task RemoveTeamMember_ServiceThrows_Returns500()
        {
            var mock = new Mock<IProjectService>();
            mock.Setup(s => s.RemoveTeamMemberAsync(It.IsAny<int>(), It.IsAny<int>())).ThrowsAsync(new Exception("fail"));
            var controller = CreateController(mock);
            var result = await controller.RemoveTeamMember(1, 2);
            var obj = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, obj.StatusCode);
        }

        [Fact]
        public async Task GetUserProjects_ServiceThrows_Returns500()
        {
            var mock = new Mock<IProjectService>();
            mock.Setup(s => s.GetUserProjectsAsync(It.IsAny<int>())).ThrowsAsync(new Exception("fail"));
            var controller = CreateController(mock);
            var result = await controller.GetUserProjects(1);
            var obj = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, obj.StatusCode);
        }
        [Fact]
        public async Task GetProjects_ReturnsPagedResult()
        {
            var mock = new Mock<IProjectService>();
            mock.Setup(s => s.GetProjectsAsync(It.IsAny<ProjectFilterDto>())).ReturnsAsync(new PagedResult<ProjectDto> { Items = new List<ProjectDto> { new ProjectDto { Id = 1 } } });
            var controller = CreateController(mock);
            var result = await controller.GetProjects(new ProjectFilterDto());
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var dto = Assert.IsType<PagedResult<ProjectDto>>(ok.Value);
            Assert.Single(dto.Items);
        }

        [Fact]
        public async Task GetProject_ReturnsOk_WhenFound()
        {
            var serviceMock = new Mock<IProjectService>();
            serviceMock.Setup(s => s.GetProjectByIdAsync(1)).ReturnsAsync(new ProjectDto { Id = 1 });
            var controller = CreateController(serviceMock);
            var result = await controller.GetProject(1);
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var dto = Assert.IsType<ProjectDto>(ok.Value);
            Assert.Equal(1, dto.Id);
        }

        [Fact]
        public async Task GetProject_ReturnsNotFound_WhenMissing()
        {
            var serviceMock = new Mock<IProjectService>();
            serviceMock.Setup(s => s.GetProjectByIdAsync(123)).ReturnsAsync((ProjectDto)null);
            var controller = CreateController();
            var result = await controller.GetProject(123);
            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        public async Task GetProjectByCode_ReturnsOk_WhenFound()
        {
            var mock = new Mock<IProjectService>();
            mock.Setup(s => s.GetProjectByCodeAsync("X")).ReturnsAsync(new ProjectDto { Id = 2, Code = "X" });
            var controller = CreateController(mock);
            var result = await controller.GetProjectByCode("X");
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var dto = Assert.IsType<ProjectDto>(ok.Value);
            Assert.Equal("X", dto.Code);
        }

        [Fact]
        public async Task GetProjectByCode_ReturnsNotFound_WhenMissing()
        {
            var mock = new Mock<IProjectService>();
            mock.Setup(s => s.GetProjectByCodeAsync("none")).ReturnsAsync((ProjectDto)null);
            var controller = CreateController(mock);
            var result = await controller.GetProjectByCode("none");
            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        public async Task CreateProject_ReturnsCreated()
        {
            var mock = new Mock<IProjectService>();
            mock.Setup(s => s.CreateProjectAsync(It.IsAny<CreateProjectDto>())).ReturnsAsync(new ProjectDto { Id = 10 });
            var controller = CreateController(mock);
            var dto = new CreateProjectDto { Name = "N", Code = "C", Status = ProjectStatus.Active, OwnerId = 1 };
            var result = await controller.CreateProject(dto);
            var created = Assert.IsType<CreatedAtActionResult>(result.Result);
            var project = Assert.IsType<ProjectDto>(created.Value);
            Assert.Equal(10, project.Id);
        }

        [Fact]
        public async Task UpdateProject_ReturnsOk_WhenFound()
        {
            var mock = new Mock<IProjectService>();
            mock.Setup(s => s.UpdateProjectAsync(1, It.IsAny<UpdateProjectDto>())).ReturnsAsync(new ProjectDto { Id = 1 });
            var controller = CreateController(mock);
            var dto = new UpdateProjectDto { Name = "U", Code = "C", Status = ProjectStatus.Active, OwnerId = 1 };
            var result = await controller.UpdateProject(1, dto);
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var project = Assert.IsType<ProjectDto>(ok.Value);
            Assert.Equal(1, project.Id);
        }

        [Fact]
        public async Task UpdateProject_ReturnsNotFound_WhenMissing()
        {
            var mock = new Mock<IProjectService>();
            mock.Setup(s => s.UpdateProjectAsync(999, It.IsAny<UpdateProjectDto>())).ReturnsAsync((ProjectDto)null);
            var controller = CreateController(mock);
            var dto = new UpdateProjectDto { Name = "U", Code = "C", Status = ProjectStatus.Active, OwnerId = 1 };
            var result = await controller.UpdateProject(999, dto);
            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        public async Task DeleteProject_ReturnsNoContent_WhenDeleted()
        {
            var mock = new Mock<IProjectService>();
            mock.Setup(s => s.DeleteProjectAsync(1)).ReturnsAsync(true);
            var controller = CreateController(mock);
            var result = await controller.DeleteProject(1);
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteProject_ReturnsNotFound_WhenMissing()
        {
            var mock = new Mock<IProjectService>();
            mock.Setup(s => s.DeleteProjectAsync(999)).ReturnsAsync(false);
            var controller = CreateController(mock);
            var result = await controller.DeleteProject(999);
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task GetProjectTeamMembers_ReturnsOk()
        {
            var mock = new Mock<IProjectService>();
            // Mock project exists to bypass the NotFound check
            mock.Setup(s => s.GetProjectByIdAsync(It.IsAny<int>())).ReturnsAsync(new ProjectDto { Id = 1 });
            mock.Setup(s => s.GetProjectTeamMembersAsync(1)).ReturnsAsync(new List<ProjectTeamMemberDto> { new ProjectTeamMemberDto { UserId = 1 } });
            var controller = CreateController(mock);
            var result = await controller.GetProjectTeamMembers(1);
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var team = Assert.IsType<List<ProjectTeamMemberDto>>(ok.Value);
            Assert.Single(team);
        }

        [Fact]
        public async Task AddTeamMember_ReturnsOk_WhenAdded()
        {
            var mock = new Mock<IProjectService>();
            mock.Setup(s => s.AddTeamMemberAsync(1, It.IsAny<AddProjectTeamMemberDto>())).ReturnsAsync(new ProjectTeamMemberDto { UserId = 2 });
            var controller = CreateController(mock);
            var result = await controller.AddTeamMember(1, new AddProjectTeamMemberDto { UserId = 2, Role = ProjectRole.QAEngineer });
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var member = Assert.IsType<ProjectTeamMemberDto>(ok.Value);
            Assert.Equal(2, member.UserId);
        }

        [Fact]
        public async Task AddTeamMember_ReturnsNotFound_WhenNull()
        {
            var mock = new Mock<IProjectService>();
            mock.Setup(s => s.AddTeamMemberAsync(1, It.IsAny<AddProjectTeamMemberDto>())).ReturnsAsync((ProjectTeamMemberDto)null);
            var controller = CreateController(mock);
            var result = await controller.AddTeamMember(1, new AddProjectTeamMemberDto { UserId = 99, Role = ProjectRole.QAEngineer });
            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        public async Task UpdateTeamMember_ReturnsOk_WhenUpdated()
        {
            var mock = new Mock<IProjectService>();
            mock.Setup(s => s.UpdateTeamMemberAsync(1, 2, It.IsAny<UpdateProjectTeamMemberDto>())).ReturnsAsync(new ProjectTeamMemberDto { UserId = 2 });
            var controller = CreateController(mock);
            var result = await controller.UpdateTeamMember(1, 2, new UpdateProjectTeamMemberDto { Role = ProjectRole.QAEngineer, IsActive = true });
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var member = Assert.IsType<ProjectTeamMemberDto>(ok.Value);
            Assert.Equal(2, member.UserId);
        }

        [Fact]
        public async Task UpdateTeamMember_ReturnsNotFound_WhenMissing()
        {
            var mock = new Mock<IProjectService>();
            mock.Setup(s => s.UpdateTeamMemberAsync(1, 2, It.IsAny<UpdateProjectTeamMemberDto>())).ReturnsAsync((ProjectTeamMemberDto)null);
            var controller = CreateController(mock);
            var result = await controller.UpdateTeamMember(1, 2, new UpdateProjectTeamMemberDto { Role = ProjectRole.QAEngineer, IsActive = true });
            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        public async Task RemoveTeamMember_ReturnsNoContent_WhenRemoved()
        {
            var mock = new Mock<IProjectService>();
            mock.Setup(s => s.RemoveTeamMemberAsync(1, 2)).ReturnsAsync(true);
            var controller = CreateController(mock);
            var result = await controller.RemoveTeamMember(1, 2);
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task RemoveTeamMember_ReturnsNotFound_WhenMissing()
        {
            var mock = new Mock<IProjectService>();
            mock.Setup(s => s.RemoveTeamMemberAsync(1, 2)).ReturnsAsync(false);
            var controller = CreateController(mock);
            var result = await controller.RemoveTeamMember(1, 2);
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task GetUserProjects_ReturnsOk()
        {
            var mock = new Mock<IProjectService>();
            mock.Setup(s => s.GetUserProjectsAsync(1)).ReturnsAsync(new List<ProjectDto> { new ProjectDto { Id = 1 } });
            var controller = CreateController(mock);
            var result = await controller.GetUserProjects(1);
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var projects = Assert.IsType<List<ProjectDto>>(ok.Value);
            Assert.Single(projects);
        }

        [Fact]
        public async Task GetProjectRequirements_ReturnsOk()
        {
            var projectServiceMock = new Mock<IProjectService>();
            var requirementServiceMock = new Mock<IRequirementService>();
            
            // Mock project exists
            projectServiceMock.Setup(s => s.GetProjectByIdAsync(1))
                .ReturnsAsync(new ProjectDto { Id = 1, Name = "Test Project" });
            
            var emptyPagedResult = new PagedResult<RequirementDto>
            {
                Items = new List<RequirementDto>(),
                TotalItems = 0,
                PageNumber = 1,
                PageSize = 10
            };
            requirementServiceMock.Setup(s => s.GetPagedAsync(It.IsAny<PaginationParameters>()))
                .ReturnsAsync(emptyPagedResult);
            
            var controller = CreateController(projectServiceMock, requirementServiceMock);
            var result = await controller.GetProjectRequirements(1);
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var paged = Assert.IsType<PagedResult<RequirementDto>>(ok.Value);
            Assert.Equal(0, paged.TotalItems);
        }

        [Fact]
        public async Task GetProjectTestSuites_ReturnsOk()
        {
            var controller = CreateController();
            var result = await controller.GetProjectTestSuites(1);
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var paged = Assert.IsType<PagedResult<TestSuiteDto>>(ok.Value);
            Assert.Equal(0, paged.TotalItems);
        }

        [Fact]
        public async Task GetProjectTestPlans_ReturnsOk()
        {
            var controller = CreateController();
            var result = await controller.GetProjectTestPlans(1);
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var paged = Assert.IsType<PagedResult<TestPlanDto>>(ok.Value);
            Assert.Equal(0, paged.TotalItems);
        }

        [Fact]
        public async Task GetNextRequirementId_ReturnsOk()
        {
            var mock = new Mock<IProjectService>();
            mock.Setup(s => s.GenerateNextRequirementIdAsync(1)).ReturnsAsync("P-REQ-001");
            var controller = CreateController(mock);
            var result = await controller.GetNextRequirementId(1);
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal("P-REQ-001", ok.Value);
        }

        [Fact]
        public async Task GetNextRequirementId_ReturnsNotFound_OnArgumentException()
        {
            var mock = new Mock<IProjectService>();
            mock.Setup(s => s.GenerateNextRequirementIdAsync(1)).ThrowsAsync(new ArgumentException("Project not found"));
            var controller = CreateController(mock);
            var result = await controller.GetNextRequirementId(1);
            Assert.IsType<NotFoundObjectResult>(result.Result);
        }
    }
}
