using Microsoft.Extensions.DependencyInjection;
using SkiaSharpGames.GameEngine;

namespace SkiaSharpGames.TwoZeroFourEight;

public static class TwoZeroFourEightGame
{
    public static Game Create()
    {
        var builder = GameBuilder.CreateDefault();

        builder.SetGameDimensions(TwoZeroFourEightConstants.GameWidth, TwoZeroFourEightConstants.GameHeight);
        builder.Services.AddSingleton<TwoZeroFourEightGameState>();

        builder.Screens
               .Add<StartScreen>()
               .Add<PlayScreen>()
               .Add<GameOverScreen>();

        builder.SetInitialScreen<StartScreen>();

        return builder.Build();
    }
}
