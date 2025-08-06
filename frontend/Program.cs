using frontend.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using frontend;
using MudBlazor.Services;
using RqmtMgmtShared;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Configure HttpClient to point to backend API
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddMudServices();

// Register services as interfaces
builder.Services.AddScoped<IRequirementService, RequirementsDataService>();
builder.Services.AddScoped<ITestCaseService, TestCasesDataService>();
builder.Services.AddScoped<ITestPlanService, TestPlansDataService>();
builder.Services.AddScoped<ITestSuiteService, TestSuitesDataService>();
builder.Services.AddScoped<IUserService, UsersDataService>();
builder.Services.AddScoped<IRequirementTestCaseLinkService, RequirementTestCaseLinkService>();
builder.Services.AddScoped<IRoleService, RolesDataService>();
builder.Services.AddScoped<IDashboardService, DashboardDataService>();
builder.Services.AddScoped<IEnhancedDashboardService, EnhancedDashboardDataService>();
builder.Services.AddScoped<ITestRunSessionDataService, TestRunSessionDataService>();
builder.Services.AddScoped<ITestExecutionDataService, TestExecutionDataService>();

await builder.Build().RunAsync();