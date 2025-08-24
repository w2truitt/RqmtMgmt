using frontend.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using frontend;
using MudBlazor.Services;
using RqmtMgmtShared;
using System.Text.Json;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Configure JSON serializer options to handle string enums consistently with backend
builder.Services.Configure<JsonSerializerOptions>(options =>
{
    options.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
});

// Configure OIDC authentication for HTTPS
builder.Services.AddOidcAuthentication(options =>
{
    builder.Configuration.Bind("OIDC", options.ProviderOptions);
    
    // Override with HTTPS configuration
    options.ProviderOptions.Authority = "https://rqmtmgmt.local";
    options.ProviderOptions.ClientId = "rqmtmgmt-frontend";
    options.ProviderOptions.ResponseType = "code";
    
    // Clear and set scopes
    options.ProviderOptions.DefaultScopes.Clear();
    options.ProviderOptions.DefaultScopes.Add("openid");
    options.ProviderOptions.DefaultScopes.Add("profile");
    options.ProviderOptions.DefaultScopes.Add("email");
    options.ProviderOptions.DefaultScopes.Add("role");
    options.ProviderOptions.DefaultScopes.Add("rqmtmgmt.api");
    
    // Set redirect URIs for HTTPS
    options.ProviderOptions.PostLogoutRedirectUri = "https://rqmtmgmt.local/";
    options.ProviderOptions.RedirectUri = "https://rqmtmgmt.local/authentication/login-callback";
});

// Configure HttpClient to point to backend API with authentication
builder.Services.AddHttpClient("RqmtMgmtAPI", client =>
    {
        client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress);
    })
    .AddHttpMessageHandler<BaseAddressAuthorizationMessageHandler>();

// Configure the default HttpClient to use the authenticated one
builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>()
    .CreateClient("RqmtMgmtAPI"));

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