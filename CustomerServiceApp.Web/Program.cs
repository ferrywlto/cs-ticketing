using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using CustomerServiceApp.Web;
using CustomerServiceApp.Web.State;
using CustomerServiceApp.Web.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Configure HTTP client with base address pointing to API
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("https://localhost:7285") });

// Register application services
builder.Services.AddSingleton<AppStateStore>();
builder.Services.AddScoped<ApiService>();

await builder.Build().RunAsync();
