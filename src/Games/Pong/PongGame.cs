using Microsoft.Extensions.DependencyInjection;

using SkiaSharp;
using SkiaSharp.Theatre;

namespace SkiaSharpGames.Pong;

public static class PongGame
{
    public static Stage Create()
    {
        var builder = StageBuilder.Create();

        builder.SetStageSize(PongConstants.GameWidth, PongConstants.GameHeight);
        builder.Services.AddSingleton<PongGameState>();

        builder.Scenes.Add<StartScreen>().Add<PlayScreen>().Add<GameOverScreen>();

        builder.SetOpeningScene<StartScreen>();

        var stage = builder.Open();
        stage.HudTheme = new HudTheme
        {
            Pointer = new CrosshairPointerAppearance
            {
                AccentColor = PongConstants.AccentColor,
            },
        };
        return stage;
    }
}
