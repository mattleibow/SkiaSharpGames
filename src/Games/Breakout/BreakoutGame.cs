using Microsoft.Extensions.DependencyInjection;

using SkiaSharp;
using SkiaSharp.Theatre;

namespace SkiaSharpGames.Breakout;

public static class BreakoutGame
{
    public static Stage Create()
    {
        var builder = StageBuilder.Create();

        builder.Services.AddSingleton<BreakoutGameState>();

        builder
            .Scenes.Add<StartScreen>()
            .Add<PlayScreen>()
            .Add<GameOverScreen>()
            .Add<VictoryScreen>();

        builder.SetStageSize(BreakoutConstants.GameWidth, BreakoutConstants.GameHeight);

        builder.SetOpeningScene<StartScreen>();

        var stage = builder.Open();
        stage.HudTheme = new HudTheme
        {
            Pointer = new CrosshairPointerAppearance
            {
                AccentColor = BreakoutConstants.AccentColor,
            },
        };
        return stage;
    }
}
