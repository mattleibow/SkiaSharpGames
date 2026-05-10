using Microsoft.Extensions.DependencyInjection;
using SkiaSharp.Theatre;

namespace SkiaSharpGames.Asteroids;

public static class AsteroidsGame
{
    public static Stage Create()
    {
        var builder = StageBuilder.Create();

        builder.SetStageSize(AsteroidsConstants.GameWidth, AsteroidsConstants.GameHeight);
        builder.Services.AddSingleton<AsteroidsGameState>();

        builder.Scenes.Add<StartScreen>().Add<PlayScreen>().Add<GameOverScreen>();

        builder.SetOpeningScene<StartScreen>();

        return builder.Open();
    }
}
