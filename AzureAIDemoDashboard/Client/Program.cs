using AzureAIDemoDashboard.Client;
using AzureAIDemoDashboard.Client.Shared;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(_ => new HttpClient {BaseAddress = new(builder.HostEnvironment.BaseAddress)});
builder.Services.AddLocalization();
builder.Services.AddSingleton<CultureHelper>();
builder.Services.AddSingleton<SpeechService>();

var app = builder.Build();


var cultureHelper = app.Services.GetRequiredService<CultureHelper>();
var culture = await cultureHelper.GetCultureAsync();
Console.WriteLine(culture);
await cultureHelper.SetCultureAsync(culture);

await app.RunAsync();