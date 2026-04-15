using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using SkiaSharpGames.BlazorApp;
using SkiaSharpGames.Breakout;
using SkiaSharpGames.CastleAttack;
using SkiaSharpGames.SinkSub;
using SkiaSharpGames.Pong;
using SkiaSharpGames.GameEngine;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Each game is registered as a keyed transient so every page visit gets a fresh Game instance
// built by its own isolated GameBuilder.
builder.Services.AddKeyedTransient<Game>("breakout", (sp, _) => BreakoutGame.Create());
builder.Services.AddKeyedTransient<Game>("castle-attack", (sp, _) => CastleAttackGame.Create());
builder.Services.AddKeyedTransient<Game>("sink-sub", (sp, _) => SinkSubGame.Create());
builder.Services.AddKeyedTransient<Game>("pong", (sp, _) => PongGame.Create());

await builder.Build().RunAsync();
