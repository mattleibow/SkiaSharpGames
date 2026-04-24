using Microsoft.Extensions.DependencyInjection;

using SkiaSharp.Theatre;

namespace SkiaSharpGames.Starfall;

public static class StarfallGame
{
    public static Stage Create()
    {
        var builder = StageBuilder.Create();

        builder.SetStageSize(StarfallConstants.GameWidth, StarfallConstants.GameHeight);
        builder.Services.AddSingleton<StarfallGameState>();

        builder.Scenes
            .Add<TitleScreen>()
            .Add<PlayScreen>()
            .Add<UpgradeScreen>()
            .Add<GameOverScreen>()
            .Add<VictoryScreen>();

        builder.SetOpeningScene<TitleScreen>();

        return builder.Open();
    }
}
