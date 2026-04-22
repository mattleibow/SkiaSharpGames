using Microsoft.Extensions.DependencyInjection;
using SkiaSharpGames.GameEngine;

namespace SkiaSharpGames.Snake;

public static class SnakeGame
{
    public static Stage Create()
    {
        var builder = Theatre.Create();

        builder.SetStageSize(SnakeConstants.GameWidth, SnakeConstants.GameHeight);
        builder.Services.AddSingleton<SnakeGameState>();

        builder.Scenes
               .Add<StartScreen>()
               .Add<PlayScreen>()
               .Add<GameOverScreen>();

        builder.SetOpeningScene<StartScreen>();

        return builder.Open();
    }
}
