using backend.Controllers;
using backend.DTOs;
using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace backend.Tests
{
    /// <summary>
    /// Unit tests for <see cref="UserController"/>.
    /// </summary>
    public class UserControllerTests
    {
        private readonly Mock<IUserService> _mockService;
        private readonly UserController _controller;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserControllerTests"/> class.
        /// </summary>
        public UserControllerTests()
        {
            _mockService = new Mock<IUserService>();
            _controller = new UserController(_mockService.Object);
        }

        /// <summary>
        /// Verifies that GetAll returns OkResult with a list of users.
        /// </summary>
        [Fact]
        public async Task GetAll_ReturnsOk_WithListOfUsers()
        {
            var users = new List<User>
            {
                new User { Id = 1, UserName = "alice", Email = "alice@example.com", Role = UserRole.Admin },
                new User { Id = 2, UserName = "bob", Email = "bob@example.com", Role = UserRole.Developer }
            };
            _mockService.Setup(s => s.GetAllAsync()).ReturnsAsync(users);
            var result = await _controller.GetAll();
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var value = Assert.IsAssignableFrom<IEnumerable<UserDto>>(okResult.Value);
            Assert.Collection(value,
                item => Assert.Equal("alice", item.UserName),
                item => Assert.Equal("bob", item.UserName));
        }

        /// <summary>
        /// Verifies that GetById returns NotFound when the user does not exist.
        /// </summary>
        [Fact]
        public async Task GetById_ReturnsNotFound_WhenNotExists()
        {
            _mockService.Setup(s => s.GetByIdAsync(42)).ReturnsAsync((User)null);
            var result = await _controller.GetById(42);
            Assert.IsType<NotFoundResult>(result.Result);
        }
    }
}
