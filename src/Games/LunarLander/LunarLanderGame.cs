using Microsoft.Extensions.DependencyInjection;

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

        return builder.Open();
    }
}