using SkiaSharpGames.GameEngine;

namespace SkiaSharpGames.BlazorApp.Games.Breakout;

/// <summary>
/// The Breakout game. Register as a transient service and inject into the game page:
/// <code>builder.Services.AddTransient&lt;BreakoutGame&gt;();</code>
/// </summary>
public sealed class BreakoutGame : Game
{
    protected override void Configure(GameBuilder builder)
    {
        builder.Services.AddSingleton<BreakoutGameState>();

        builder.AddScreen<BreakoutStartScreen>()
               .AddScreen<BreakoutPlayScreen>()
               .AddScreen<BreakoutGameOverScreen>()
               .AddScreen<BreakoutVictoryScreen>();
    }
}
