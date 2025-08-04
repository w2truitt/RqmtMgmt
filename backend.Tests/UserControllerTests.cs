using System;
using backend.Controllers;
using RqmtMgmtShared;
using backend.Models;
using backend.Services;
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
        public async Task GetAll_ReturnsOk_WithListOfUsers()
        {
            var users = new List<User>
            {
                new User { Id = 1, UserName = "alice", Email = "alice@example.com", Role = "Admin" },
                new User { Id = 2, UserName = "bob", Email = "bob@example.com", Role = "Developer" }
            };
            _mockService.Setup(s => s.GetAllAsync()).ReturnsAsync(users);
            var result = await _controller.GetAll();
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var value = Assert.IsAssignableFrom<IEnumerable<UserDto>>(okResult.Value);
            Assert.Collection(value,
                item => Assert.Equal("alice", item.UserName),
                item => Assert.Equal("bob", item.UserName));
        }

        [Fact]
        public async Task GetById_ReturnsNotFound_WhenNotExists()
        {
            _mockService.Setup(s => s.GetByIdAsync(42)).ReturnsAsync((User)null);
            var result = await _controller.GetById(42);
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task Create_ReturnsCreated_WithUser()
        {
            var dto = new UserDto { UserName = "alice", Email = "alice@example.com", Role = "Admin" };
            var entity = new User { Id = 1, UserName = "alice", Email = "alice@example.com", Role = "Admin" };
            _mockService.Setup(s => s.CreateAsync(It.IsAny<User>())).ReturnsAsync(entity);
            var result = await _controller.Create(dto);
            var created = Assert.IsType<CreatedAtActionResult>(result.Result);
            var value = Assert.IsAssignableFrom<UserDto>(created.Value);
            Assert.Equal("alice", value.UserName);
        }

        [Fact]
        public async Task Update_ReturnsOk_WithUser()
        {
            var dto = new UserDto { Id = 1, UserName = "updated", Email = "updated@example.com", Role = "User" };
            var entity = new User { Id = 1, UserName = "updated", Email = "updated@example.com", Role = "User" };
            _mockService.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(entity);
            _mockService.Setup(s => s.UpdateAsync(It.IsAny<User>())).ReturnsAsync(entity);
            var result = await _controller.Update(1, dto);
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var value = Assert.IsAssignableFrom<UserDto>(ok.Value);
            Assert.Equal("updated", value.UserName);
        }

        [Fact]
        public async Task Update_ReturnsNotFound_WhenNotExists()
        {
            var dto = new UserDto { Id = 99, UserName = "notfound", Email = "notfound@example.com", Role = "User" };
            _mockService.Setup(s => s.GetByIdAsync(99)).ReturnsAsync((User)null);
            var result = await _controller.Update(99, dto);
            Assert.IsType<NotFoundResult>(result.Result);
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
    }
}
