using Microsoft.Extensions.DependencyInjection;
using SkiaSharpGames.GameEngine;

namespace SkiaSharpGames.SinkSub;

public static class SinkSubGame
{
    public static Stage Create()
    {
        var builder = Theatre.Create();

        builder.SetStageSize(SinkSubConstants.GameWidth, SinkSubConstants.GameHeight);
        builder.Services.AddSingleton<SinkSubGameState>();

        builder.Scenes
               .Add<StartScreen>()
               .Add<PlayScreen>()
               .Add<GameOverScreen>();

        builder.SetOpeningScene<StartScreen>();

        return builder.Open();
    }
}
