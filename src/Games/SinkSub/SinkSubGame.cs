using Microsoft.Extensions.DependencyInjection;

using SkiaSharp;
using SkiaSharp.Theatre;

namespace SkiaSharpGames.SinkSub;

public static class SinkSubGame
{
    public static Stage Create()
    {
        var builder = StageBuilder.Create();

        builder.SetStageSize(SinkSubConstants.GameWidth, SinkSubConstants.GameHeight);
        builder.Services.AddSingleton<SinkSubGameState>();

        builder.Scenes.Add<StartScreen>().Add<PlayScreen>().Add<GameOverScreen>();

        builder.SetOpeningScene<StartScreen>();

        var stage = builder.Open();
        stage.HudTheme = new HudTheme
        {
            Pointer = new CrosshairPointerAppearance
            {
                AccentColor = SinkSubConstants.AccentColor,
                ShadowColor = new SKColor(0x08, 0x2E, 0x5A, 200),
            },
        };
        return stage;
    }
}
