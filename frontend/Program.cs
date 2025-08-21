using frontend.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using frontend;
using MudBlazor.Services;
using RqmtMgmtShared;
using System.Text.Json;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.Extensions.DependencyInjection;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Configure JSON serializer options to handle string enums consistently with backend
builder.Services.Configure<JsonSerializerOptions>(options =>
{
    options.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
});

// Configure OIDC authentication for IdentityServer
builder.Services.AddOidcAuthentication(options =>
{
    builder.Configuration.Bind("Local", options.ProviderOptions);
    options.ProviderOptions.Authority = "http://localhost:8080";
    options.ProviderOptions.ClientId = "rqmtmgmt-wasm";
    options.ProviderOptions.ResponseType = "code";
    options.ProviderOptions.DefaultScopes.Add("openid");
    options.ProviderOptions.DefaultScopes.Add("profile");
    options.ProviderOptions.DefaultScopes.Add("email");
    options.ProviderOptions.DefaultScopes.Add("rqmtapi");
});

// Configure HttpClient with authentication
builder.Services.AddHttpClient("RqmtMgmt.ServerAPI", client => 
    client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress))
    .AddHttpMessageHandler<BaseAddressAuthorizationMessageHandler>();

// Register HttpClient for authenticated API calls
builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>()
    .CreateClient("RqmtMgmt.ServerAPI"));

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
builder.Services.AddScoped<IProjectService, ProjectsDataService>();
builder.Services.AddScoped<IProjectContextService, ProjectContextService>();

await builder.Build().RunAsync();