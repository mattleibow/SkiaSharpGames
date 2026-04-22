using Microsoft.Extensions.DependencyInjection;
using SkiaSharpGames.GameEngine;

namespace SkiaSharpGames.CastleAttack;

public static class CastleAttackGame
{
    public static Stage Create()
    {
        var builder = Theatre.Create();

        builder.SetStageSize(CastleAttackConstants.GameWidth, CastleAttackConstants.GameHeight);
        builder.Services.AddSingleton<CastleAttackGameState>();

        builder.Scenes
               .Add<StartScreen>()
               .Add<PlayScreen>()
               .Add<GameOverScreen>()
               .Add<VictoryScreen>();

        builder.SetOpeningScene<StartScreen>();

        return builder.Open();
    }
}
