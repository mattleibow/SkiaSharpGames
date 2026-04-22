using Microsoft.Extensions.DependencyInjection;
using SkiaSharpGames.GameEngine;
using SkiaSharpGames.GameEngine.UI;

namespace SkiaSharpGames.UIGallery;

public static class UIGalleryGame
{
    public static Game Create()
    {
        var builder = GameBuilder.CreateDefault();

        builder.SetGameDimensions(800f, 600f);
        builder.Services.AddSingleton<UIGalleryState>();
        builder.SetUiTheme(UiThemes.Simple);

        builder.Screens.Add<PlayScreen>();
        builder.SetInitialScreen<PlayScreen>();

        return builder.Build();
    }
}
