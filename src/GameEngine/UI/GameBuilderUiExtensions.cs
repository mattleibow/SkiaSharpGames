using Microsoft.Extensions.DependencyInjection;

namespace SkiaSharpGames.GameEngine.UI;

public static class GameBuilderUiExtensions
{
    public static GameBuilder SetUiTheme(this GameBuilder builder, UiTheme theme)
    {
        builder.Services.AddSingleton(theme);
        return builder;
    }
}
