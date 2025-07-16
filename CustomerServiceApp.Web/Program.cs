using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using CustomerServiceApp.Web;
using CustomerServiceApp.Web.State;
using CustomerServiceApp.Web.Services;
using CustomerServiceApp.Web.Configuration;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Configure settings from appsettings.json
var apiSettings = builder.Configuration.GetSection(ApiSettings.SectionName).Get<ApiSettings>() 
    ?? throw new InvalidOperationException("ApiSettings section is missing from configuration");

// Configure HTTP client with base address from configuration
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(apiSettings.BaseUrl) });
builder.Services.AddLogging();

// Register configuration settings
builder.Services.Configure<ApiSettings>(builder.Configuration.GetSection(ApiSettings.SectionName));

// Register application services
builder.Services.AddSingleton<AppStateStore>();
builder.Services.AddSingleton<ILocalStorageService, LocalStorageService>();
builder.Services.AddScoped<ApiService>();

await builder.Build().RunAsync();
