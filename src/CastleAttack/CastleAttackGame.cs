using Microsoft.Extensions.DependencyInjection;
using SkiaSharpGames.GameEngine;

namespace SkiaSharpGames.CastleAttack;

public static class CastleAttackGame
{
    public static Game Create()
    {
        var builder = GameBuilder.CreateDefault();

        builder.SetGameDimensions(CastleAttackConstants.GameWidth, CastleAttackConstants.GameHeight);
        builder.Services.AddSingleton<CastleAttackGameState>();

        builder.Screens
               .Add<StartScreen>()
               .Add<PlayScreen>()
               .Add<GameOverScreen>()
               .Add<VictoryScreen>();

        builder.SetInitialScreen<StartScreen>();

        return builder.Build();
    }
}
