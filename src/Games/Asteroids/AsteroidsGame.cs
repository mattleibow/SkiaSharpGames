using Microsoft.Extensions.DependencyInjection;
using SkiaSharpGames.GameEngine;

namespace SkiaSharpGames.Asteroids;

public static class AsteroidsGame
{
    public static Stage Create()
    {
        var builder = Theatre.Create();

        builder.SetStageSize(AsteroidsConstants.GameWidth, AsteroidsConstants.GameHeight);
        builder.Services.AddSingleton<AsteroidsGameState>();

        builder.Scenes
               .Add<StartScreen>()
               .Add<PlayScreen>()
               .Add<GameOverScreen>();

        builder.SetOpeningScene<StartScreen>();

        return builder.Open();
    }
}
