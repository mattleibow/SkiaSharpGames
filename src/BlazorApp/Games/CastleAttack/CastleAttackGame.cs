using SkiaSharpGames.GameEngine;

namespace SkiaSharpGames.BlazorApp.Games.CastleAttack;

internal static class CastleAttackGame
{
    internal static Game Create()
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
