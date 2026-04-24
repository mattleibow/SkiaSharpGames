using Microsoft.Extensions.DependencyInjection;

using SkiaSharp.Theatre;
using SkiaSharp.Theatre.Themes.Default;

namespace SkiaSharpGames.UIGallery;

public static class UIGalleryGame
{
    public static Stage Create()
    {
        var builder = StageBuilder.Create();

        builder.SetStageSize(800f, 600f);
        builder.Services.AddSingleton<UIGalleryState>();

        builder.Scenes.Add<PlayScreen>();
        builder.SetOpeningScene<PlayScreen>();

        var stage = builder.Open();
        stage.HudTheme = DefaultTheme.Create();
        return stage;
    }
}
