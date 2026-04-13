using SkiaSharpGames.GameEngine;

namespace SkiaSharpGames.BlazorApp.Games.Breakout;

internal static class BreakoutGame
{
    internal static Game Create()
    {
        var builder = GameBuilder.CreateDefault();

        builder.Services.AddSingleton<BreakoutGameState>();

        builder.Screens
               .Add<BreakoutStartScreen>()
               .Add<BreakoutPlayScreen>()
               .Add<BreakoutGameOverScreen>()
               .Add<BreakoutVictoryScreen>();

        return builder.Build();
    }
}
