using System;
using backend.Controllers;
using RqmtMgmtShared;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace backend.Tests
{
    public class RoleControllerTests
    {
        private readonly Mock<IRoleService> _mockService;
        private readonly RoleController _controller;

        public RoleControllerTests()
        {
            _mockService = new Mock<IRoleService>();
            _controller = new RoleController(_mockService.Object);
        }

        [Fact]
        public async Task GetAllRoles_ReturnsOkResult_WithListOfRoles()
        {
            var roles = new List<RoleDto> { new RoleDto { Id = 1, Name = "Admin" }, new RoleDto { Id = 2, Name = "User" } };
            _mockService.Setup(s => s.GetAllRolesAsync()).ReturnsAsync(roles);
            
            var result = await _controller.GetAllRoles();
            
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var value = Assert.IsAssignableFrom<List<RoleDto>>(okResult.Value);
            Assert.Collection(value,
                item => Assert.Equal("Admin", item.Name),
                item => Assert.Equal("User", item.Name));
        }

        [Fact]
        public async Task CreateRole_ReturnsOkResult()
        {
            var roleDto = new RoleDto { Id = 3, Name = "NewRole" };
            _mockService.Setup(s => s.CreateRoleAsync("NewRole")).ReturnsAsync(roleDto);
            var result = await _controller.CreateRole("NewRole");
            Assert.IsType<OkObjectResult>(result.Result);
        }

        [Fact]
        public async Task DeleteRole_ReturnsNoContent()
        {
            _mockService.Setup(s => s.DeleteRoleAsync(It.IsAny<int>())).ReturnsAsync(true);
            var result = await _controller.DeleteRole(3);
            Assert.IsType<NoContentResult>(result);
        }
    }
}