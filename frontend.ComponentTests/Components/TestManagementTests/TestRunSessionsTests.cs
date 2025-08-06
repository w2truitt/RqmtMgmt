using Bunit;
using frontend.Pages;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using RqmtMgmtShared;
using frontend.Services;
using Xunit;
using AngleSharp.Dom;

namespace frontend.ComponentTests.Components.TestManagementTests;

/// <summary>
/// Tests for the TestRunSessions component
/// </summary>
public class TestRunSessionsTests : ComponentTestBase
{
    [Fact]
    public void TestRunSessions_RendersCorrectly_WithTitle()
    {
        // Arrange
        SetupMockServices();
        
        // Act
        var component = RenderComponent<TestRunSessions>();
        
        // Assert
        Assert.Contains("Test Run Sessions", component.Markup);
        Assert.Contains("<h1>Test Run Sessions</h1>", component.Markup);
    }
    
    [Fact]
    public void TestRunSessions_ShowsLoadingSpinner_WhenLoading()
    {
        // Arrange
        var mockService = GetMockService<ITestRunSessionDataService>();
        mockService.Setup(s => s.GetAllAsync()).Returns(Task.Delay(1000).ContinueWith(_ => new List<TestRunSessionDto>()));
        
        SetupOtherMockServices();
        
        // Act
        var component = RenderComponent<TestRunSessions>();
        
        // Assert
        var spinner = component.Find(".spinner-border");
        Assert.NotNull(spinner);
        Assert.Contains("Loading...", component.Markup);
    }
    
    [Fact]
    public void TestRunSessions_DisplaysCreateButton()
    {
        // Arrange
        SetupMockServices();
        
        // Act
        var component = RenderComponent<TestRunSessions>();
        
        // Assert
        var createButton = component.FindAll("button").FirstOrDefault(b => b.TextContent.Contains("Create New Session"));
        Assert.NotNull(createButton);
        Assert.Contains("bi-plus-circle", createButton.InnerHtml);
    }
    
    [Fact]
    public void TestRunSessions_DisplaysSearchAndFilterControls()
    {
        // Arrange
        SetupMockServices();
        
        // Act
        var component = RenderComponent<TestRunSessions>();
        
        // Assert
        var searchInput = component.Find("input[placeholder*='Search sessions']");
        var statusFilter = component.Find("select.form-select");
        var refreshButton = component.FindAll("button").FirstOrDefault(b => b.TextContent.Contains("Refresh"));
        
        Assert.NotNull(searchInput);
        Assert.NotNull(statusFilter);
        Assert.NotNull(refreshButton);
        Assert.Contains("bi-arrow-clockwise", refreshButton?.InnerHtml ?? "");
    }
    
    [Fact]
    public void TestRunSessions_DisplaysSessionsTable()
    {
        // Arrange
        var mockSessions = CreateMockSessions();
        SetupMockServices(mockSessions);
        
        // Act
        var component = RenderComponent<TestRunSessions>();
        
        // Assert
        var table = component.Find("table");
        Assert.NotNull(table);
        
        // Check table headers
        var headers = table.QuerySelectorAll("th");
        var headerTexts = headers.Select(h => h.TextContent).ToList();
        
        Assert.Contains("Name", headerTexts);
        Assert.Contains("Test Plan", headerTexts);
        Assert.Contains("Status", headerTexts);
        Assert.Contains("Executor", headerTexts);
        Assert.Contains("Started", headerTexts);
        Assert.Contains("Completed", headerTexts);
        Assert.Contains("Environment", headerTexts);
        Assert.Contains("Build Version", headerTexts);
        Assert.Contains("Actions", headerTexts);
    }
    
    [Fact]
    public void TestRunSessions_DisplaysSessionData()
    {
        // Arrange
        var mockSessions = CreateMockSessions();
        SetupMockServices(mockSessions);
        
        // Act
        var component = RenderComponent<TestRunSessions>();
        
        // Assert
        Assert.Contains("Test Session 1", component.Markup);
        Assert.Contains("Test Plan Alpha", component.Markup);
        Assert.Contains("John Doe", component.Markup);
        Assert.Contains("Production", component.Markup);
        Assert.Contains("v1.2.3", component.Markup);
    }
    
    [Fact]
    public void TestRunSessions_DisplaysCorrectStatusBadges()
    {
        // Arrange
        var mockSessions = CreateMockSessions();
        SetupMockServices(mockSessions);
        
        // Act
        var component = RenderComponent<TestRunSessions>();
        
        // Assert
        var badges = component.FindAll(".badge");
        Assert.True(badges.Count >= 2);
        
        // Check for status badges
        var inProgressBadge = badges.FirstOrDefault(b => b.TextContent.Contains("InProgress"));
        var completedBadge = badges.FirstOrDefault(b => b.TextContent.Contains("Completed"));
        
        Assert.NotNull(inProgressBadge);
        Assert.NotNull(completedBadge);
        Assert.Contains("bg-primary", inProgressBadge.GetAttribute("class") ?? "");
        Assert.Contains("bg-success", completedBadge.GetAttribute("class") ?? "");
    }
    
    [Fact]
    public void TestRunSessions_DisplaysActionButtons()
    {
        // Arrange
        var mockSessions = CreateMockSessions();
        SetupMockServices(mockSessions);
        
        // Act
        var component = RenderComponent<TestRunSessions>();
        
        // Assert
        var actionButtons = component.FindAll(".btn-group button");
        Assert.True(actionButtons.Count > 0);
        
        // Check for specific action icons
        var viewButtons = actionButtons.Where(b => b.QuerySelector(".bi-eye") != null);
        var playButtons = actionButtons.Where(b => b.QuerySelector(".bi-play-fill") != null);
        var completeButtons = actionButtons.Where(b => b.QuerySelector(".bi-check-circle") != null);
        var abortButtons = actionButtons.Where(b => b.QuerySelector(".bi-x-circle") != null);
        
        Assert.True(viewButtons.Any());
        Assert.True(playButtons.Any());
        Assert.True(completeButtons.Any());
        Assert.True(abortButtons.Any());
    }
    
    [Fact]
    public void TestRunSessions_ShowsCreateModal_WhenCreateButtonClicked()
    {
        // Arrange
        SetupMockServices();
        var component = RenderComponent<TestRunSessions>();
        
        // Act
        var createButton = component.FindAll("button").First(b => b.TextContent.Contains("Create New Session"));
        createButton.Click();
        
        // Assert
        var modal = component.Find(".modal");
        Assert.NotNull(modal);
        Assert.Contains("Create Test Run Session", component.Markup);
        
        // Check modal form fields
        var nameInput = component.Find("input#sessionName");
        var descriptionInput = component.Find("textarea#sessionDescription");
        var testPlanSelect = component.Find("select#testPlan");
        var executorSelect = component.Find("select#executor");
        
        Assert.NotNull(nameInput);
        Assert.NotNull(descriptionInput);
        Assert.NotNull(testPlanSelect);
        Assert.NotNull(executorSelect);
    }
    
    [Fact]
    public void TestRunSessions_HidesModal_WhenCancelClicked()
    {
        // Arrange
        SetupMockServices();
        var component = RenderComponent<TestRunSessions>();
        
        // Show modal first
        var createButton = component.FindAll("button").First(b => b.TextContent.Contains("Create New Session"));
        createButton.Click();
        
        // Act
        var cancelButton = component.FindAll("button").First(b => b.TextContent.Contains("Cancel"));
        cancelButton.Click();
        
        // Assert
        var modals = component.FindAll(".modal");
        Assert.Empty(modals);
    }
    
    [Fact]
    public void TestRunSessions_FiltersSessionsByStatus()
    {
        // Arrange
        var mockSessions = CreateMockSessions();
        SetupMockServices(mockSessions);
        var component = RenderComponent<TestRunSessions>();
        
        // Act
        var statusFilter = component.Find("select.form-select");
        statusFilter.Change("InProgress");
        
        // Assert
        // Should only show InProgress sessions
        var rows = component.FindAll("tbody tr");
        Assert.True(rows.Count > 0);
        
        foreach (var row in rows)
        {
            var statusBadge = row.QuerySelector(".badge");
            Assert.NotNull(statusBadge);
            Assert.Contains("InProgress", statusBadge.TextContent);
        }
    }
    
    [Fact]
    public void TestRunSessions_ShowsEmptyMessage_WhenNoSessionsFound()
    {
        // Arrange
        SetupMockServices(new List<TestRunSessionDto>());
        
        // Act
        var component = RenderComponent<TestRunSessions>();
        
        // Assert
        Assert.Contains("No test run sessions found", component.Markup);
    }
    
    [Fact]
    public void TestRunSessions_LoadsTestPlansAndUsers_ForModal()
    {
        // Arrange
        var mockTestPlans = new List<TestPlanDto>
        {
            new TestPlanDto { Id = 1, Name = "Test Plan 1", Type = "Unit" },
            new TestPlanDto { Id = 2, Name = "Test Plan 2", Type = "Integration" }
        };
        
        var mockUsers = new List<UserDto>
        {
            new UserDto { Id = 1, UserName = "User1", Email = "user1@example.com" },
            new UserDto { Id = 2, UserName = "User2", Email = "user2@example.com" }
        };
        
        SetupMockServices(testPlans: mockTestPlans, users: mockUsers);
        var component = RenderComponent<TestRunSessions>();
        
        // Act
        var createButton = component.FindAll("button").First(b => b.TextContent.Contains("Create New Session"));
        createButton.Click();
        
        // Assert
        var testPlanOptions = component.FindAll("select#testPlan option");
        var executorOptions = component.FindAll("select#executor option");
        
        Assert.True(testPlanOptions.Count >= 3); // Default option + 2 test plans
        Assert.True(executorOptions.Count >= 3); // Default option + 2 users
        
        Assert.Contains("Test Plan 1", testPlanOptions.Select(o => o.TextContent));
        Assert.Contains("User1", executorOptions.Select(o => o.TextContent));
    }
    
    private List<TestRunSessionDto> CreateMockSessions()
    {
        return new List<TestRunSessionDto>
        {
            new TestRunSessionDto
            {
                Id = 1,
                Name = "Test Session 1",
                Description = "First test session",
                TestPlanId = 1,
                TestPlanName = "Test Plan Alpha",
                ExecutedBy = 1,
                ExecutorName = "John Doe",
                StartedAt = DateTime.Now.AddHours(-2),
                Status = TestRunStatus.InProgress,
                Environment = "Production",
                BuildVersion = "v1.2.3"
            },
            new TestRunSessionDto
            {
                Id = 2,
                Name = "Test Session 2",
                Description = "Second test session",
                TestPlanId = 2,
                TestPlanName = "Test Plan Beta",
                ExecutedBy = 2,
                ExecutorName = "Jane Smith",
                StartedAt = DateTime.Now.AddDays(-1),
                CompletedAt = DateTime.Now.AddHours(-1),
                Status = TestRunStatus.Completed,
                Environment = "Staging",
                BuildVersion = "v1.2.2"
            }
        };
    }
    
    private void SetupMockServices(List<TestRunSessionDto>? sessions = null, 
                                  List<TestPlanDto>? testPlans = null, 
                                  List<UserDto>? users = null)
    {
        var mockSessionService = GetMockService<ITestRunSessionDataService>();
        var mockTestPlanService = GetMockService<ITestPlanService>();
        var mockUserService = GetMockService<IUserService>();
        
        mockSessionService.Setup(s => s.GetAllAsync())
            .ReturnsAsync(sessions ?? CreateMockSessions());
        
        mockTestPlanService.Setup(s => s.GetAllAsync())
            .ReturnsAsync(testPlans ?? new List<TestPlanDto>
            {
                new TestPlanDto { Id = 1, Name = "Default Test Plan", Type = "Unit" }
            });
        
        mockUserService.Setup(s => s.GetAllAsync())
            .ReturnsAsync(users ?? new List<UserDto>
            {
                new UserDto { Id = 1, UserName = "Default User", Email = "default@example.com" }
            });
        
        mockSessionService.Setup(s => s.CreateAsync(It.IsAny<TestRunSessionDto>()))
            .ReturnsAsync(new TestRunSessionDto { Id = 999, Name = "Created Session" });
        
        mockSessionService.Setup(s => s.CompleteTestRunSessionAsync(It.IsAny<int>()))
            .ReturnsAsync(true);
        
        mockSessionService.Setup(s => s.AbortTestRunSessionAsync(It.IsAny<int>()))
            .ReturnsAsync(true);
    }
    
    private void SetupOtherMockServices()
    {
        var mockTestPlanService = GetMockService<ITestPlanService>();
        var mockUserService = GetMockService<IUserService>();
        
        mockTestPlanService.Setup(s => s.GetAllAsync())
            .ReturnsAsync(new List<TestPlanDto>());
        
        mockUserService.Setup(s => s.GetAllAsync())
            .ReturnsAsync(new List<UserDto>());
    }
}