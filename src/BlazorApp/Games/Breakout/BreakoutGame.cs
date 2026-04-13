using SkiaSharp;
using SkiaSharpGames.GameEngine;

namespace SkiaSharpGames.BlazorApp.Games.Breakout;

internal static class BreakoutGame
{
    internal static Game Create()
    {
        var builder = GameBuilder.CreateDefault();

        builder.SetGameDimensions(new SKSize(BreakoutConstants.GameWidth, BreakoutConstants.GameHeight));
        builder.Services.AddSingleton<BreakoutGameState>();

        builder.Screens
               .Add<BreakoutStartScreen>()
               .Add<BreakoutPlayScreen>()
               .Add<BreakoutGameOverScreen>()
               .Add<BreakoutVictoryScreen>();

        builder.SetInitialScreen<BreakoutStartScreen>();

        return builder.Build();
    }
}
