using SkiaSharpGames.GameEngine;

namespace SkiaSharpGames.BlazorApp.Games.Breakout;

internal static class BreakoutGame
{
    internal static Game Create()
    {
        var builder = GameBuilder.CreateDefault();

        builder.SetGameDimensions(BreakoutConstants.GameWidth, BreakoutConstants.GameHeight);
        builder.Services.AddSingleton<BreakoutGameState>();

        builder.Screens
               .Add<StartScreen>()
               .Add<PlayScreen>()
               .Add<GameOverScreen>()
               .Add<VictoryScreen>();

        builder.SetInitialScreen<StartScreen>();

        return builder.Build();
    }
}
