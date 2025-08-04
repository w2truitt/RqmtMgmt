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
        public async Task GetLinksForRequirement_ReturnsOk_WithLinks()
        {
            var links = new List<RequirementTestCaseLinkDto> { new RequirementTestCaseLinkDto { RequirementId = 1, TestCaseId = 2 } };
            _mockService.Setup(s => s.GetLinksForRequirementAsync(1)).ReturnsAsync(links);
            var result = await _controller.GetLinksForRequirement(1);
            var ok = Assert.IsType<OkObjectResult>(result);
            var value = Assert.IsAssignableFrom<IEnumerable<RequirementTestCaseLinkDto>>(ok.Value);
            Assert.Single(value);
        }

        [Fact]
        public async Task GetLinksForTestCase_ReturnsOk_WithLinks()
        {
            var links = new List<RequirementTestCaseLinkDto> { new RequirementTestCaseLinkDto { RequirementId = 1, TestCaseId = 2 } };
            _mockService.Setup(s => s.GetLinksForTestCaseAsync(2)).ReturnsAsync(links);
            var result = await _controller.GetLinksForTestCase(2);
            var ok = Assert.IsType<OkObjectResult>(result);
            var value = Assert.IsAssignableFrom<IEnumerable<RequirementTestCaseLinkDto>>(ok.Value);
            Assert.Single(value);
        }

        [Fact]
        public async Task CreateLink_ReturnsCreated()
        {
            var dto = new RequirementTestCaseLinkDto { RequirementId = 1, TestCaseId = 2 };
            _mockService.Setup(s => s.CreateLinkAsync(dto)).ReturnsAsync(true);
            var result = await _controller.CreateLink(dto);
            Assert.IsType<CreatedResult>(result);
        }

        [Fact]
        public async Task CreateLink_ReturnsBadRequest_WhenNotCreated()
        {
            var dto = new RequirementTestCaseLinkDto { RequirementId = 1, TestCaseId = 2 };
            _mockService.Setup(s => s.CreateLinkAsync(dto)).ReturnsAsync(false);
            var result = await _controller.CreateLink(dto);
            Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public async Task DeleteLink_ReturnsNoContent_WhenDeleted()
        {
            _mockService.Setup(s => s.DeleteLinkAsync(1, 2)).ReturnsAsync(true);
            var result = await _controller.DeleteLink(1, 2);
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteLink_ReturnsNotFound_WhenNotExists()
        {
            _mockService.Setup(s => s.DeleteLinkAsync(1, 2)).ReturnsAsync(false);
            var result = await _controller.DeleteLink(1, 2);
            Assert.IsType<NotFoundResult>(result);
        }
    }
}
