using Microsoft.Extensions.DependencyInjection;
using SkiaSharp.Theatre;

namespace SkiaSharpGames.SpaceInvaders;

public static class SpaceInvadersGame
{
    public static Stage Create()
    {
        var builder = StageBuilder.Create();

        builder.SetStageSize(SpaceInvadersConstants.GameWidth, SpaceInvadersConstants.GameHeight);
        builder.Services.AddSingleton<SpaceInvadersGameState>();

        builder
            .Scenes.Add<StartScreen>()
            .Add<PlayScreen>()
            .Add<GameOverScreen>()
            .Add<VictoryScreen>();

        builder.SetOpeningScene<StartScreen>();

        return builder.Open();
    }
}
