using Microsoft.Extensions.DependencyInjection;
using SkiaSharpGames.GameEngine;
using SkiaSharpGames.GameEngine.UI;

namespace SkiaSharpGames.UIGallery;

public static class UIGalleryGame
{
    public static Stage Create()
    {
        var builder = Theatre.Create();

        builder.SetStageSize(800f, 600f);
        builder.Services.AddSingleton<UIGalleryState>();
        builder.SetUiTheme(UiThemes.Simple);

        builder.Scenes.Add<PlayScreen>();
        builder.SetOpeningScene<PlayScreen>();

        return builder.Open();
    }
}
