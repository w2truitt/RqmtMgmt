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

        #region CreateRole Tests

        [Fact]
        public async Task CreateRole_ReturnsBadRequest_WhenServiceReturnsNull()
        {
            _mockService.Setup(s => s.CreateRoleAsync("InvalidRole")).ReturnsAsync((RoleDto)null);
            var result = await _controller.CreateRole("InvalidRole");
            Assert.IsType<BadRequestResult>(result.Result);
        }

        [Fact]
        public async Task CreateRole_ReturnsOkWithRole_WhenSuccessful()
        {
            var roleDto = new RoleDto { Id = 3, Name = "ValidRole" };
            _mockService.Setup(s => s.CreateRoleAsync("ValidRole")).ReturnsAsync(roleDto);
            var result = await _controller.CreateRole("ValidRole");
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedRole = Assert.IsType<RoleDto>(okResult.Value);
            Assert.Equal("ValidRole", returnedRole.Name);
        }

        #endregion

        #region CreateRoleFromDto Tests

        [Fact]
        public async Task CreateRoleFromDto_ReturnsOkResult_WhenSuccessful()
        {
            var inputDto = new RoleDto { Name = "DtoRole" };
            var returnedDto = new RoleDto { Id = 4, Name = "DtoRole" };
            _mockService.Setup(s => s.CreateRoleAsync("DtoRole")).ReturnsAsync(returnedDto);
            
            var result = await _controller.CreateRoleFromDto(inputDto);
            
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var value = Assert.IsType<RoleDto>(okResult.Value);
            Assert.Equal("DtoRole", value.Name);
            Assert.Equal(4, value.Id);
        }

        [Fact]
        public async Task CreateRoleFromDto_ReturnsBadRequest_WhenServiceReturnsNull()
        {
            var inputDto = new RoleDto { Name = "InvalidDtoRole" };
            _mockService.Setup(s => s.CreateRoleAsync("InvalidDtoRole")).ReturnsAsync((RoleDto)null);
            
            var result = await _controller.CreateRoleFromDto(inputDto);
            
            Assert.IsType<BadRequestResult>(result.Result);
        }

        #endregion

        #region DeleteRole Tests

        [Fact]
        public async Task DeleteRole_ReturnsNotFound_WhenServiceReturnsFalse()
        {
            _mockService.Setup(s => s.DeleteRoleAsync(999)).ReturnsAsync(false);
            var result = await _controller.DeleteRole(999);
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteRole_ReturnsNoContent_WhenServiceReturnsTrue()
        {
            _mockService.Setup(s => s.DeleteRoleAsync(1)).ReturnsAsync(true);
            var result = await _controller.DeleteRole(1);
            Assert.IsType<NoContentResult>(result);
        }

        #endregion

        #region GetAllRoles Tests

        [Fact]
        public async Task GetAllRoles_ReturnsEmptyList_WhenNoRoles()
        {
            var roles = new List<RoleDto>();
            _mockService.Setup(s => s.GetAllRolesAsync()).ReturnsAsync(roles);
            
            var result = await _controller.GetAllRoles();
            
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var value = Assert.IsAssignableFrom<List<RoleDto>>(okResult.Value);
            Assert.Empty(value);
        }

        #endregion
    }
}