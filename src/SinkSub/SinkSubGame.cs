using Microsoft.Extensions.DependencyInjection;
using SkiaSharpGames.GameEngine;

namespace SkiaSharpGames.SinkSub;

public static class SinkSubGame
{
    public static Game Create()
    {
        var builder = GameBuilder.CreateDefault();

        builder.SetGameDimensions(SinkSubConstants.GameWidth, SinkSubConstants.GameHeight);
        builder.Services.AddSingleton<SinkSubGameState>();

        builder.Screens
               .Add<StartScreen>()
               .Add<PlayScreen>()
               .Add<GameOverScreen>();

        builder.SetInitialScreen<StartScreen>();

        return builder.Build();
    }
}
