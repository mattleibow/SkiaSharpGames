using Microsoft.Extensions.DependencyInjection;

using SkiaSharp;
using SkiaSharp.Theatre;

namespace SkiaSharpGames.Catch;

public static class CatchGame
{
    public static Stage Create()
    {
        var builder = StageBuilder.Create();

        builder.SetStageSize(CatchConstants.GameWidth, CatchConstants.GameHeight);
        builder.Services.AddSingleton<CatchGameState>();

        builder.Scenes.Add<StartScreen>().Add<PlayScreen>().Add<GameOverScreen>();

        builder.SetOpeningScene<StartScreen>();

        var stage = builder.Open();
        stage.HudTheme = new HudTheme
        {
            Pointer = new CrosshairPointerAppearance
            {
                AccentColor = CatchConstants.AccentColor,
            },
        };
        return stage;
    }
}
