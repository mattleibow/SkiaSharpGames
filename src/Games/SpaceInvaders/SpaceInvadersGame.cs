using Microsoft.Extensions.DependencyInjection;
using SkiaSharpGames.GameEngine;

namespace SkiaSharpGames.SpaceInvaders;

public static class SpaceInvadersGame
{
    public static Game Create()
    {
        var builder = GameBuilder.CreateDefault();

        builder.SetGameDimensions(SpaceInvadersConstants.GameWidth, SpaceInvadersConstants.GameHeight);
        builder.Services.AddSingleton<SpaceInvadersGameState>();

        builder.Screens
               .Add<StartScreen>()
               .Add<PlayScreen>()
               .Add<GameOverScreen>()
               .Add<VictoryScreen>();

        builder.SetInitialScreen<StartScreen>();

        return builder.Build();
    }
}
