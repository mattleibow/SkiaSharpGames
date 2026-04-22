using Microsoft.Extensions.DependencyInjection;
using SkiaSharpGames.GameEngine;

namespace SkiaSharpGames.Pong;

public static class PongGame
{
    public static Game Create()
    {
        var builder = GameBuilder.CreateDefault();

        builder.SetGameDimensions(PongConstants.GameWidth, PongConstants.GameHeight);
        builder.Services.AddSingleton<PongGameState>();

        builder.Screens
               .Add<StartScreen>()
               .Add<PlayScreen>()
               .Add<GameOverScreen>();

        builder.SetInitialScreen<StartScreen>();

        return builder.Build();
    }
}
