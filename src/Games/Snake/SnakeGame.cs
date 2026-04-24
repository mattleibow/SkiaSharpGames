using Microsoft.Extensions.DependencyInjection;

using SkiaSharp.Theatre;

namespace SkiaSharpGames.Snake;

public static class SnakeGame
{
    public static Stage Create()
    {
        var builder = StageBuilder.Create();

        builder.SetStageSize(SnakeConstants.GameWidth, SnakeConstants.GameHeight);
        builder.Services.AddSingleton<SnakeGameState>();

        builder.Scenes.Add<StartScreen>().Add<PlayScreen>().Add<GameOverScreen>();

        builder.SetOpeningScene<StartScreen>();

        return builder.Open();
    }
}