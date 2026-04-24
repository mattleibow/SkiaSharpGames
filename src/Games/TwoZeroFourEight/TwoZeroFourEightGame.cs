using Microsoft.Extensions.DependencyInjection;

using SkiaSharp;
using SkiaSharp.Theatre;

namespace SkiaSharpGames.TwoZeroFourEight;

public static class TwoZeroFourEightGame
{
    public static Stage Create()
    {
        var builder = StageBuilder.Create();

        builder.SetStageSize(
            TwoZeroFourEightConstants.GameWidth,
            TwoZeroFourEightConstants.GameHeight
        );
        builder.Services.AddSingleton<TwoZeroFourEightGameState>();

        builder.Scenes.Add<StartScreen>().Add<PlayScreen>().Add<GameOverScreen>();

        builder.SetOpeningScene<StartScreen>();

        var stage = builder.Open();
        stage.HudTheme = new HudTheme
        {
            Pointer = new CrosshairPointerAppearance
            {
                AccentColor = TwoZeroFourEightConstants.HeaderColor,
                ShadowColor = new SKColor(0xFF, 0xFF, 0xFF, 180),
            },
        };
        return stage;
    }
}
