using Microsoft.Extensions.DependencyInjection;
using SkiaSharpGames.GameEngine;

namespace SkiaSharpGames.Breakout;

public static class BreakoutGame
{
    public static Game Create()
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
