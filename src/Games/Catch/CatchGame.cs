using Microsoft.Extensions.DependencyInjection;
using SkiaSharpGames.GameEngine;

namespace SkiaSharpGames.Catch;

public static class CatchGame
{
    public static Game Create()
    {
        var builder = GameBuilder.CreateDefault();

        builder.SetGameDimensions(CatchConstants.GameWidth, CatchConstants.GameHeight);
        builder.Services.AddSingleton<CatchGameState>();

        builder.Screens
               .Add<StartScreen>()
               .Add<PlayScreen>()
               .Add<GameOverScreen>();

        builder.SetInitialScreen<StartScreen>();

        return builder.Build();
    }
}
