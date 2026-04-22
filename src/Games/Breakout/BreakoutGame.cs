using Microsoft.Extensions.DependencyInjection;
using SkiaSharp.Theatre;

namespace SkiaSharpGames.Breakout;

public static class BreakoutGame
{
    public static Stage Create()
    {
        var builder = StageBuilder.Create();

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
