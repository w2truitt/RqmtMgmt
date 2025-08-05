using Moq;
using RqmtMgmtShared;

namespace frontend.ComponentTests.TestHelpers;

/// <summary>
/// Helper methods for setting up mock services
/// </summary>
public static class MockServiceHelpers
{
    /// <summary>
    /// Sets up common mock behaviors for IRequirementService
    /// </summary>
    public static void SetupRequirementServiceMock(Mock<IRequirementService> mockService)
    {
        mockService.Setup(s => s.GetAllAsync())
            .ReturnsAsync(new List<RequirementDto>
            {
                new RequirementDto { Id = 1, Title = "Test Requirement 1", Type = "CRS", Status = "Active" },
                new RequirementDto { Id = 2, Title = "Test Requirement 2", Type = "PRS", Status = "Draft" }
            });

        mockService.Setup(s => s.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((int id) => new RequirementDto { Id = id, Title = $"Requirement {id}", Type = "CRS", Status = "Active" });

        mockService.Setup(s => s.CreateAsync(It.IsAny<RequirementDto>()))
            .ReturnsAsync((RequirementDto req) => 
            {
                req.Id = 999;
                return req;
            });

        mockService.Setup(s => s.UpdateAsync(It.IsAny<RequirementDto>()))
            .ReturnsAsync(true);

        mockService.Setup(s => s.DeleteAsync(It.IsAny<int>()))
            .ReturnsAsync(true);
    }

    /// <summary>
    /// Sets up common mock behaviors for ITestCaseService
    /// </summary>
    public static void SetupTestCaseServiceMock(Mock<ITestCaseService> mockService)
    {
        mockService.Setup(s => s.GetAllAsync())
            .ReturnsAsync(new List<TestCaseDto>
            {
                new TestCaseDto { Id = 1, Title = "Test Case 1", Description = "Test Description 1" },
                new TestCaseDto { Id = 2, Title = "Test Case 2", Description = "Test Description 2" }
            });

        mockService.Setup(s => s.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((int id) => new TestCaseDto { Id = id, Title = $"Test Case {id}", Description = $"Description {id}" });

        mockService.Setup(s => s.CreateAsync(It.IsAny<TestCaseDto>()))
            .ReturnsAsync((TestCaseDto tc) => 
            {
                tc.Id = 999;
                return tc;
            });

        mockService.Setup(s => s.UpdateAsync(It.IsAny<TestCaseDto>()))
            .ReturnsAsync(true);

        mockService.Setup(s => s.DeleteAsync(It.IsAny<int>()))
            .ReturnsAsync(true);
    }

    /// <summary>
    /// Sets up common mock behaviors for IUserService
    /// </summary>
    public static void SetupUserServiceMock(Mock<IUserService> mockService)
    {
        mockService.Setup(s => s.GetAllAsync())
            .ReturnsAsync(new List<UserDto>
            {
                new UserDto { Id = 1, UserName = "Test User 1", Email = "user1@test.com" },
                new UserDto { Id = 2, UserName = "Test User 2", Email = "user2@test.com" }
            });

        mockService.Setup(s => s.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((int id) => new UserDto { Id = id, UserName = $"User {id}", Email = $"user{id}@test.com" });

        mockService.Setup(s => s.CreateAsync(It.IsAny<UserDto>()))
            .ReturnsAsync((UserDto user) => 
            {
                user.Id = 999;
                return user;
            });

        mockService.Setup(s => s.UpdateAsync(It.IsAny<UserDto>()))
            .ReturnsAsync(true);

        mockService.Setup(s => s.DeleteAsync(It.IsAny<int>()))
            .ReturnsAsync(true);
    }
}