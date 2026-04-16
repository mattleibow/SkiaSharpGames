using Microsoft.Extensions.DependencyInjection;

namespace SkiaSharpGames.GameEngine.UI;

public static class GameBuilderUiExtensions
{
    public static GameBuilder SetUiTheme(this GameBuilder builder, UiTheme theme)
    {
        builder.Services.AddSingleton<IUiThemeProvider>(_ => new UiThemeProvider(theme));
        return builder;
    }
}
