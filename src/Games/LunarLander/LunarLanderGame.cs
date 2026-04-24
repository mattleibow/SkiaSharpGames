using Microsoft.Extensions.DependencyInjection;

using SkiaSharp;
using SkiaSharp.Theatre;

namespace SkiaSharpGames.LunarLander;

public static class LunarLanderGame
{
    public static Stage Create()
    {
        var builder = StageBuilder.Create();

        builder.SetStageSize(LunarLanderConstants.GameWidth, LunarLanderConstants.GameHeight);
        builder.Services.AddSingleton<LunarLanderGameState>();

        builder.Scenes.Add<StartScreen>().Add<PlayScreen>().Add<GameOverScreen>();

        builder.SetOpeningScene<StartScreen>();

        var stage = builder.Open();
        stage.HudTheme = new HudTheme
        {
            Pointer = new CrosshairPointerAppearance
            {
                AccentColor = LunarLanderConstants.AccentColor,
            },
        };
        return stage;
    }
}
