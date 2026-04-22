using Microsoft.Extensions.DependencyInjection;
using SkiaSharpGames.GameEngine;

namespace SkiaSharpGames.Breakout;

public static class BreakoutGame
{
    public static Stage Create()
    {
        var builder = Theatre.Create();

        builder.SetStageSize(BreakoutConstants.GameWidth, BreakoutConstants.GameHeight);
        builder.Services.AddSingleton<BreakoutGameState>();

        builder.Scenes
               .Add<StartScreen>()
               .Add<PlayScreen>()
               .Add<GameOverScreen>()
               .Add<VictoryScreen>();

        builder.SetOpeningScene<StartScreen>();

        return builder.Open();
    }
}
