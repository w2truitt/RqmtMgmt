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
            var roles = new List<string> { "Admin", "User" };
            _mockService.Setup(s => s.GetAllRolesAsync()).ReturnsAsync(roles);
            
            var result = await _controller.GetAllRoles();
            
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var value = Assert.IsAssignableFrom<List<string>>(okResult.Value);
            Assert.Collection(value,
                item => Assert.Equal("Admin", item),
                item => Assert.Equal("User", item));
        }

        [Fact]
        public async Task CreateRole_ReturnsNoContent()
        {
            _mockService.Setup(s => s.CreateRoleAsync(It.IsAny<string>())).Returns(Task.CompletedTask);
            
            var result = await _controller.CreateRole("NewRole");
            
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteRole_ReturnsNoContent()
        {
            _mockService.Setup(s => s.DeleteRoleAsync(It.IsAny<string>())).Returns(Task.CompletedTask);
            
            var result = await _controller.DeleteRole("RoleToDelete");
            
            Assert.IsType<NoContentResult>(result);
        }
    }
}