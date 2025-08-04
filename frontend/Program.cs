using frontend.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using frontend;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddMudServices();

builder.Services.AddScoped<RequirementsDataService>();
builder.Services.AddScoped<TestCasesDataService>();
builder.Services.AddScoped<TestPlansDataService>();
builder.Services.AddScoped<TestSuitesDataService>();
builder.Services.AddScoped<UsersDataService>();
builder.Services.AddScoped<RequirementTestCaseLinkService>();
builder.Services.AddScoped<RolesDataService>();
await builder.Build().RunAsync();