using Microsoft.Extensions.DependencyInjection;
using SkiaSharpGames.GameEngine;

namespace SkiaSharpGames.Catch;

public static class CatchGame
{
    public static Stage Create()
    {
        var builder = Theatre.Create();

        builder.SetStageSize(CatchConstants.GameWidth, CatchConstants.GameHeight);
        builder.Services.AddSingleton<CatchGameState>();

        builder.Scenes
               .Add<StartScreen>()
               .Add<PlayScreen>()
               .Add<GameOverScreen>();

        builder.SetOpeningScene<StartScreen>();

        return builder.Open();
    }
}
