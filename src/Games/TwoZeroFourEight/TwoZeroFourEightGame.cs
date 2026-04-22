using Microsoft.Extensions.DependencyInjection;
using SkiaSharpGames.GameEngine;

namespace SkiaSharpGames.TwoZeroFourEight;

public static class TwoZeroFourEightGame
{
    public static Stage Create()
    {
        var builder = Theatre.Create();

        builder.SetStageSize(TwoZeroFourEightConstants.GameWidth, TwoZeroFourEightConstants.GameHeight);
        builder.Services.AddSingleton<TwoZeroFourEightGameState>();

        builder.Scenes
               .Add<StartScreen>()
               .Add<PlayScreen>()
               .Add<GameOverScreen>();

        builder.SetOpeningScene<StartScreen>();

        return builder.Open();
    }
}
