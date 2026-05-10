using Microsoft.Extensions.DependencyInjection;
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

        return builder.Open();
    }
}
