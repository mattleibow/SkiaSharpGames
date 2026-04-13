using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using SkiaSharpGames.BlazorApp;
using SkiaSharpGames.BlazorApp.Games.Breakout;
using SkiaSharpGames.BlazorApp.Games.CastleAttack;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Games are registered as transient so each page visit gets a fresh game instance.
builder.Services.AddTransient<BreakoutGame>();
builder.Services.AddTransient<CastleAttackGame>();

await builder.Build().RunAsync();
