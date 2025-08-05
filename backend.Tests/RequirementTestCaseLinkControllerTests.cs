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
    public class RequirementTestCaseLinkControllerTests
    {
        private readonly Mock<IRequirementTestCaseLinkService> _mockService;
        private readonly RequirementTestCaseLinkController _controller;

        public RequirementTestCaseLinkControllerTests()
        {
            _mockService = new Mock<IRequirementTestCaseLinkService>();
            _controller = new RequirementTestCaseLinkController(_mockService.Object);
        }

        [Fact]
        public async Task GetLinksForRequirement_ReturnsOkResult_WithLinks()
        {
            var links = new List<RequirementTestCaseLinkDto>
            {
                new RequirementTestCaseLinkDto { RequirementId = 1, TestCaseId = 2 }
            };
            _mockService.Setup(s => s.GetLinksForRequirement(1)).ReturnsAsync(links);
            
            var result = await _controller.GetLinksForRequirement(1);
            
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var value = Assert.IsAssignableFrom<List<RequirementTestCaseLinkDto>>(okResult.Value);
            Assert.Single(value);
        }

        [Fact]
        public async Task GetLinksForTestCase_ReturnsOkResult_WithLinks()
        {
            var links = new List<RequirementTestCaseLinkDto>
            {
                new RequirementTestCaseLinkDto { RequirementId = 1, TestCaseId = 2 }
            };
            _mockService.Setup(s => s.GetLinksForTestCase(2)).ReturnsAsync(links);
            
            var result = await _controller.GetLinksForTestCase(2);
            
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var value = Assert.IsAssignableFrom<List<RequirementTestCaseLinkDto>>(okResult.Value);
            Assert.Single(value);
        }

        [Fact]
        public async Task AddLink_ReturnsNoContent()
        {
            var dto = new RequirementTestCaseLinkDto { RequirementId = 1, TestCaseId = 2 };
            _mockService.Setup(s => s.AddLink(1, 2)).Returns(Task.CompletedTask);
            
            var result = await _controller.AddLink(dto);
            
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task RemoveLink_ReturnsNoContent()
        {
            _mockService.Setup(s => s.RemoveLink(1, 2)).Returns(Task.CompletedTask);
            
            var result = await _controller.RemoveLink(1, 2);
            
            Assert.IsType<NoContentResult>(result);
        }
    }
}