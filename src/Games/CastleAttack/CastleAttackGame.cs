using Microsoft.Extensions.DependencyInjection;
using SkiaSharp.Theatre;

namespace SkiaSharpGames.CastleAttack;

public static class CastleAttackGame
{
    public static Stage Create()
    {
        var builder = StageBuilder.Create();

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
