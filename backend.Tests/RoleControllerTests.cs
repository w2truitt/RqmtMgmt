using backend.Controllers;
using backend.Models;
using backend.Services;
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
        public async Task GetAll_ReturnsAllRoles()
        {
            var roles = new List<Role> {
                new Role { Id = 1, Name = "Admin" },
                new Role { Id = 2, Name = "QA" }
            };
            _mockService.Setup(s => s.GetAllAsync()).ReturnsAsync(roles);
            var result = await _controller.GetAll();
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var value = Assert.IsAssignableFrom<List<Role>>(ok.Value);
            Assert.Contains(value, r => r.Name == "Admin");
            Assert.Contains(value, r => r.Name == "QA");
        }

        [Fact]
        public async Task Create_ReturnsCreated()
        {
            var role = new Role { Id = 1, Name = "Admin" };
            _mockService.Setup(s => s.CreateAsync("Admin")).ReturnsAsync(role);
            var result = await _controller.Create("Admin");
            var created = Assert.IsType<CreatedAtActionResult>(result.Result);
            var value = Assert.IsType<Role>(created.Value);
            Assert.Equal("Admin", value.Name);
        }

        [Fact]
        public async Task Delete_ReturnsNoContent_WhenDeleted()
        {
            _mockService.Setup(s => s.DeleteAsync(1)).ReturnsAsync(true);
            var result = await _controller.Delete(1);
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task Delete_ReturnsNotFound_WhenRoleMissing()
        {
            _mockService.Setup(s => s.DeleteAsync(99)).ReturnsAsync(false);
            var result = await _controller.Delete(99);
            Assert.IsType<NotFoundResult>(result);
        }
    }
}
