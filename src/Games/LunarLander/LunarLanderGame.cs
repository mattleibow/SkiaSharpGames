using Microsoft.Extensions.DependencyInjection;
using SkiaSharpGames.GameEngine;

namespace SkiaSharpGames.LunarLander;

public static class LunarLanderGame
{
    public static Game Create()
    {
        var builder = GameBuilder.CreateDefault();

        builder.SetGameDimensions(LunarLanderConstants.GameWidth, LunarLanderConstants.GameHeight);
        builder.Services.AddSingleton<LunarLanderGameState>();

        builder.Screens
               .Add<StartScreen>()
               .Add<PlayScreen>()
               .Add<GameOverScreen>();

        builder.SetInitialScreen<StartScreen>();

        return builder.Build();
    }
}
