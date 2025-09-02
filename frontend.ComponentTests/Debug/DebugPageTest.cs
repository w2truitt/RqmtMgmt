using Bunit;
using frontend.Pages;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using RqmtMgmtShared;
using Xunit;
using Bunit.TestDoubles;
using frontend.Services;
using Xunit.Abstractions;

namespace frontend.ComponentTests.Debug;

public class DebugPageTest : ComponentTestBase
{
    private readonly ITestOutputHelper _output;

    public DebugPageTest(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void Debug_ProjectTeam_Markup()
    {
        // Arrange
        var mockProjectService = new Mock<IProjectService>();
        var mockProjectContextService = new Mock<IProjectContextService>();
        
        Services.AddSingleton(mockProjectService.Object);
        Services.AddSingleton(mockProjectContextService.Object);
        
        var authContext = this.AddTestAuthorization();
        authContext.SetAuthorized("test@example.com", AuthorizationState.Authorized);
        
        // Set up some mock data
        var testTeamMembers = new List<ProjectTeamMemberDto>
        {
            new ProjectTeamMemberDto
            {
                ProjectId = 1,
                UserId = 1,
                UserName = "John Doe",
                UserEmail = "john.doe@example.com",
                Role = ProjectRole.Developer,
                JoinedAt = DateTime.Now.AddDays(-30),
                IsActive = true
            }
        };

        mockProjectService.Setup(x => x.GetProjectTeamMembersAsync(1))
            .ReturnsAsync(testTeamMembers);

        // Navigate to the ProjectTeam page
        var component = RenderComponent<ProjectTeam>(parameters => parameters
            .Add(p => p.ProjectId, 1));

        // Output the actual markup
        _output.WriteLine("=== ACTUAL MARKUP ===");
        _output.WriteLine(component.Markup);
        _output.WriteLine("=== END MARKUP ===");
        
        // This test always passes - it's just for debugging
        Assert.True(true);
    }
}
