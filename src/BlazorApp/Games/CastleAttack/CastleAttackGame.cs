using SkiaSharpGames.GameEngine;

namespace SkiaSharpGames.BlazorApp.Games.CastleAttack;

internal static class CastleAttackGame
{
    internal static Game Create()
    {
        var builder = GameBuilder.CreateDefault();

        builder.Services.AddSingleton<CastleAttackGameState>();

        builder.Screens
               .Add<CastleAttackStartScreen>()
               .Add<CastleAttackPlayScreen>()
               .Add<CastleAttackGameOverScreen>()
               .Add<CastleAttackVictoryScreen>();

        return builder.Build();
    }
}
