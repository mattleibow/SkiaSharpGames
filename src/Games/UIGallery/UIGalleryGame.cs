using Microsoft.Extensions.DependencyInjection;
using SkiaSharp.Theatre;

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

        return builder.Open();
    }
}
