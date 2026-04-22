using Microsoft.Extensions.DependencyInjection;

namespace SkiaSharpGames.GameEngine.UI;

public static class TheatreUiExtensions
{
    public static Theatre SetUiTheme(this Theatre builder, UiTheme theme)
    {
        builder.Services.AddSingleton(theme);
        return builder;
    }
}
