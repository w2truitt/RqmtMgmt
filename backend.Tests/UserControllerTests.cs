#nullable enable
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
    public class UserControllerTests
    {
        private readonly Mock<IUserService> _mockService;
        private readonly UserController _controller;

        public UserControllerTests()
        {
            _mockService = new Mock<IUserService>();
            _controller = new UserController(_mockService.Object);
        }

        [Fact]
        public async Task GetAll_ReturnsOkResult_WithListOfUsers()
        {
            var users = new List<UserDto>
            {
                new UserDto { Id = 1, UserName = "user1", Email = "user1@test.com" },
                new UserDto { Id = 2, UserName = "user2", Email = "user2@test.com" }
            };
            _mockService.Setup(s => s.GetAllAsync()).ReturnsAsync(users);
            
            var result = await _controller.GetAll();
            
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var value = Assert.IsAssignableFrom<List<UserDto>>(okResult.Value);
            Assert.Equal(2, value.Count);
        }

        [Fact]
        public async Task GetById_ReturnsNotFound_WhenNotExists()
        {
            _mockService.Setup(s => s.GetByIdAsync(42)).ReturnsAsync((UserDto?)null);
            
            var result = await _controller.GetById(42);
            
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task Create_ReturnsCreated_WithUser()
        {
            var dto = new UserDto { UserName = "newuser", Email = "new@test.com" };
            var entity = new UserDto { Id = 1, UserName = "newuser", Email = "new@test.com" };
            _mockService.Setup(s => s.CreateAsync(It.IsAny<UserDto>())).ReturnsAsync(entity);
            
            var result = await _controller.Create(dto);
            
            var created = Assert.IsType<CreatedAtActionResult>(result.Result);
            var value = Assert.IsAssignableFrom<UserDto>(created.Value);
            Assert.Equal("newuser", value.UserName);
        }

        [Fact]
        public async Task Update_ReturnsNoContent_WhenSuccessful()
        {
            var dto = new UserDto { Id = 1, UserName = "updated", Email = "updated@test.com" };
            _mockService.Setup(s => s.UpdateAsync(It.IsAny<UserDto>())).ReturnsAsync(true);
            
            var result = await _controller.Update(1, dto);
            
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task Update_ReturnsNotFound_WhenNotExists()
        {
            var dto = new UserDto { Id = 99, UserName = "notfound", Email = "notfound@test.com" };
            _mockService.Setup(s => s.UpdateAsync(It.IsAny<UserDto>())).ReturnsAsync(false);
            
            var result = await _controller.Update(99, dto);
            
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Delete_ReturnsNoContent_WhenDeleted()
        {
            _mockService.Setup(s => s.DeleteAsync(1)).ReturnsAsync(true);
            
            var result = await _controller.Delete(1);
            
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task Delete_ReturnsNotFound_WhenNotExists()
        {
            _mockService.Setup(s => s.DeleteAsync(99)).ReturnsAsync(false);
            
            var result = await _controller.Delete(99);
            
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task GetRoles_ReturnsOkResult_WithRoles()
        {
            var roles = new List<string> { "Admin", "User" };
            _mockService.Setup(s => s.GetUserRolesAsync(1)).ReturnsAsync(roles);
            
            var result = await _controller.GetRoles(1);
            
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var value = Assert.IsAssignableFrom<List<string>>(okResult.Value);
            Assert.Equal(2, value.Count);
        }

        [Fact]
        public async Task AssignRole_ReturnsNoContent()
        {
            _mockService.Setup(s => s.AssignRoleAsync(1, "Admin")).Returns(Task.CompletedTask);
            
            var result = await _controller.AssignRole(1, "Admin");
            
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task RemoveRole_ReturnsNoContent()
        {
            _mockService.Setup(s => s.RemoveRoleAsync(1, "Admin")).Returns(Task.CompletedTask);
            
            var result = await _controller.RemoveRole(1, "Admin");
            
            Assert.IsType<NoContentResult>(result);
        }
    }
}