using Microsoft.Extensions.DependencyInjection;
using SkiaSharpGames.GameEngine;

namespace SkiaSharpGames.Pong;

public static class PongGame
{
    public static Stage Create()
    {
        var builder = Theatre.Create();

        builder.SetStageSize(PongConstants.GameWidth, PongConstants.GameHeight);
        builder.Services.AddSingleton<PongGameState>();

        builder.Scenes
               .Add<StartScreen>()
               .Add<PlayScreen>()
               .Add<GameOverScreen>();

        builder.SetOpeningScene<StartScreen>();

        return builder.Open();
    }
}
