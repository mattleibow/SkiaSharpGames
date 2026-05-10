using Microsoft.Extensions.DependencyInjection;
using SkiaSharp.Theatre;
using static SkiaSharpGames.GhostLight.GhostLightConstants;

namespace SkiaSharpGames.GhostLight;

public static class GhostLightGame
{
    public static Stage Create()
    {
        var builder = StageBuilder.Create();

        builder.SetStageSize(GameWidth, GameHeight);
        builder.Services.AddSingleton<GhostLightState>();

        builder.Scenes.Add<StartScreen>().Add<PlayScreen>().Add<GameOverScreen>();

        builder.SetOpeningScene<StartScreen>();

        return builder.Open();
    }
}
